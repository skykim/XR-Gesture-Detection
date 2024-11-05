using System.Collections.Generic;
using UnityEngine.XR.Hands.Gestures;
using System.IO;
using System.Linq;
using System.Text;
using Unity.Sentis;
using System.Threading.Tasks;
using TMPro;
using UnityEngine.Networking;

namespace UnityEngine.XR.Hands.Samples.Gestures.DebugTools
{
    public class GestureDetector : MonoBehaviour
    {
        XRFingerShape[] m_XRFingerShapes;
        static List<XRHandSubsystem> s_SubsystemsReuse = new List<XRHandSubsystem>();
        List<string> _traingData = new List<string>();
        float[] _fingerData = new float[28*2];

        int _classID = 0;
        bool isTraining = true;

        [SerializeField]
        private TMP_Text inferenceResultText;

        //Sentis
        private string classificationModelName = "xrhands_gesture_classification.sentis";
        const BackendType backend = BackendType.GPUCompute;
        Worker worker;
        Model classificationModelWithSoftmax;

        async void Start()
        {
            m_XRFingerShapes = new XRFingerShape[(int)XRHandFingerID.Little - (int)XRHandFingerID.Thumb + 1];
            SetupModel();

            await RunInferenceAsync();
        }

        void SetupModel()
        {
            Model classificationModel = ModelLoader.Load(GetPathFromStreamingAssets(classificationModelName));

            var graph = new FunctionalGraph();
            var inputs = graph.AddInputs(classificationModel);
            FunctionalTensor[] outputs = Functional.Forward(classificationModel, inputs);
            FunctionalTensor softmaxOutput = Functional.Softmax(outputs[0]);
            classificationModelWithSoftmax = graph.Compile(softmaxOutput);

            worker = new Worker(classificationModelWithSoftmax, backend);
        }

        async Task RunInferenceAsync()
        {
            TensorShape shape = new TensorShape(1, 28*2);
            using var tensor = new Tensor<float>(shape, _fingerData);
            worker.Schedule(tensor);
            var classIndex = worker.PeekOutput() as Tensor<float>;
            using var tensorData = await classIndex.ReadbackAndCloneAsync();

            float threshold = 0.8f;

            if (tensorData[0] > threshold)
            {
                inferenceResultText.text = "I";
            }
            else if (tensorData[1] > threshold)
            {
                inferenceResultText.text = "Love";
            }
            else if (tensorData[2] > threshold)
            {
                inferenceResultText.text = "Unity";
            }
            else if (tensorData[3] > threshold)
            {
                inferenceResultText.text = "6";
            }            
            else
            {
                inferenceResultText.text = "";
            }
        }        

        bool isExecute = false;

        async void LateUpdate()
        {
            if(isExecute == true)
                return;

            var subsystem = TryGetSubsystem();
            if (subsystem == null)
                return;

            _fingerData.Initialize();

            _fingerData[0] = _classID;
            GetFingerData(subsystem.leftHand);
            GetFingerData(subsystem.rightHand);

            if (_fingerData.Sum() == 0)
            {
                return;
            }
            else
            {
                if (isTraining == false)
                {
                    await RunInferenceAsync();
                }
                else
                {
                    //save csv
                    StringBuilder sb = new StringBuilder();
                    sb.Append(_classID.ToString());
                    for (int i = 0; i < _fingerData.Length; i++)
                    {
                        sb.Append(",");
                        sb.Append(_fingerData[i].ToString("F8"));
                    }
                    Debug.Log(sb.ToString());
                    _traingData.Add(sb.ToString());
                }
            }
        }

        public void SetClassID(int id)
        {
            _classID = id;
        }

        public void StartTraining()
        {
            _traingData.Clear();
            isTraining = true;
        }

        public void StopTraining()
        {
            isTraining = false;
            using (StreamWriter writer = new StreamWriter(Application.persistentDataPath + "/points.csv", true))
            {
                foreach (string data in _traingData)
                {
                    writer.WriteLine(data);
                }
            }

            _traingData.Clear();
        }

        void GetFingerData(XRHand hand)
        {
            int offset = 0;
            int offset_left = 0;            
            int offset_right = 28;
            
            if (hand.handedness == Handedness.Left)
                offset = offset_left;
            else if (hand.handedness == Handedness.Right)
                offset = offset_right;
            else
                return;

            if (hand.GetJoint(XRHandJointID.Palm).TryGetPose(out var pose))
            {
                Vector3 palmRotation = pose.rotation.eulerAngles;
                if (palmRotation == Vector3.zero)
                {
                    return;
                }

                _fingerData[offset+0] = palmRotation.x;
                _fingerData[offset+1] = palmRotation.y;
                _fingerData[offset+2] = palmRotation.z;
            }

            for (var fingerIndex = (int)XRHandFingerID.Thumb; fingerIndex <= (int)XRHandFingerID.Little; ++fingerIndex)
            {
                m_XRFingerShapes[fingerIndex] = hand.CalculateFingerShape((XRHandFingerID)fingerIndex, XRFingerShapeTypes.All);
                var shapes = m_XRFingerShapes[fingerIndex];

                shapes.TryGetFullCurl(out var fullCurl);
                _fingerData[(offset+3) + fingerIndex*5 + 0] = fullCurl;

                shapes.TryGetBaseCurl(out var baseCurl);
                _fingerData[(offset+3) + fingerIndex*5 + 1] = baseCurl;

                shapes.TryGetTipCurl(out var tipCurl);
                _fingerData[(offset+3) + fingerIndex*5 + 2] = tipCurl;
                
                shapes.TryGetPinch(out var pinch);
                _fingerData[(offset+3) + fingerIndex*5 + 3] = pinch;

                shapes.TryGetSpread(out var spread);
                _fingerData[(offset+3) + fingerIndex*5 + 4] = spread;
            }
        }

        static XRHandSubsystem TryGetSubsystem()
        {
            SubsystemManager.GetSubsystems(s_SubsystemsReuse);
            return s_SubsystemsReuse.Count > 0 ? s_SubsystemsReuse[0] : null;
        }

        void OnDestroy()
        {
            worker?.Dispose();
            worker = null;
        }        

        string GetPathFromStreamingAssets(string path)
        {
            #if UNITY_ANDROID && !UNITY_EDITOR
            var loadingRequest = UnityWebRequest.Get(Path.Combine(Application.streamingAssetsPath, path));
            loadingRequest.SendWebRequest();
            while (!loadingRequest.isDone)
            {
                if (loadingRequest.isNetworkError || loadingRequest.isHttpError)
                {
                    break;
                }
            }
            if (loadingRequest.isNetworkError || loadingRequest.isHttpError)
            {
                return null;
            }
            else
            {
                File.WriteAllBytes(Path.Combine(Application.persistentDataPath , path), loadingRequest.downloadHandler.data);
                return Path.Combine(Application.persistentDataPath, path);
            }
            #else
            return Path.Combine(Application.streamingAssetsPath, path);
            #endif
        }        
    }
}