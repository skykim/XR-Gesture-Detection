using System.Collections.Generic;
using UnityEngine.XR.Hands.Gestures;

namespace UnityEngine.XR.Hands.Samples.Gestures.DebugTools
{
    /// <summary>
    /// Controls the debug UI for finger state values by setting that values in each <see cref="XRFingerStateDebugUI"/>.
    /// </summary>
    public class XRAllFingerShapesDebugUI : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("The handedness to get the finger states for.")]
        Handedness m_Handedness;

        [SerializeField]
        [Tooltip("The five debugs graphs for each finger in the order of Thumb to Little.")]
        XRFingerShapeDebugUI[] m_XRFingerShapeDebugGraphs = new XRFingerShapeDebugUI[5];

        XRFingerShape[] m_XRFingerShapes;

        static List<XRHandSubsystem> s_SubsystemsReuse = new List<XRHandSubsystem>();

        /// <summary>
        /// The graphs for each finger, indexed by <see cref="XRHandFingerID"/>.
        /// </summary>
        public XRFingerShapeDebugUI[] xrFingerShapeDebugGraphs => m_XRFingerShapeDebugGraphs;

        void Start()
        {
            if (m_Handedness == Handedness.Invalid)
            {
                Debug.LogWarning($"The Handedness property of { GetType() } is set to Invalid and will default to Right.", this);
                m_Handedness = Handedness.Right;
            }

            m_XRFingerShapes = new XRFingerShape[(int)XRHandFingerID.Little - (int)XRHandFingerID.Thumb + 1];
            UpdateFingerNames();
        }

        void UpdateFingerNames()
        {
            for (var i = 0; i < m_XRFingerShapeDebugGraphs.Length; i++)
                m_XRFingerShapeDebugGraphs[i].SetFingerName(((XRHandFingerID)i).ToString());
        }

        void Update()
        {
            var subsystem = TryGetSubsystem();
            if (subsystem == null)
                return;

            var hand = m_Handedness == Handedness.Left ? subsystem.leftHand : subsystem.rightHand;

            //print rotation of palm
            hand.GetJoint(XRHandJointID.Palm).TryGetPose(out var pose);
            print(m_Handedness.ToString() + ":" + pose.rotation.eulerAngles.ToString());

            for (var fingerIndex = (int)XRHandFingerID.Thumb;
                 fingerIndex <= (int)XRHandFingerID.Little;
                 ++fingerIndex)
            {
                m_XRFingerShapes[fingerIndex] = hand.CalculateFingerShape(
                    (XRHandFingerID)fingerIndex, XRFingerShapeTypes.All);
                UpdateFingerShapeUIs(fingerIndex);
            }
        }

        void UpdateFingerShapeUIs(int fingerIndex)
        {
            var graph = m_XRFingerShapeDebugGraphs[fingerIndex];
            var shapes = m_XRFingerShapes[fingerIndex];

            string curlValues = "";

            for (var i = 0; i < m_XRFingerShapeDebugGraphs.Length; i++)
            {
                if (shapes.TryGetFullCurl(out var fullCurl))
                {
                    graph.SetFingerShape((int)XRFingerShapeType.FullCurl, fullCurl);
                    curlValues = "0:" + fullCurl.ToString();
                }
                else
                    graph.HideFingerShape((int)XRFingerShapeType.FullCurl);

                if (shapes.TryGetBaseCurl(out var baseCurl))
                {
                    graph.SetFingerShape((int)XRFingerShapeType.BaseCurl, baseCurl);
                    curlValues += ", 1:" + baseCurl.ToString();
                }
                else
                    graph.HideFingerShape((int)XRFingerShapeType.BaseCurl);

                if (shapes.TryGetTipCurl(out var tipCurl))
                {
                    graph.SetFingerShape((int)XRFingerShapeType.TipCurl, tipCurl);
                    curlValues += ", 2:" + tipCurl.ToString();
                }
                else
                    graph.HideFingerShape((int)XRFingerShapeType.TipCurl);

                if (shapes.TryGetPinch(out var pinch))
                {
                    graph.SetFingerShape((int)XRFingerShapeType.Pinch, pinch);
                    curlValues += ", 3:" + pinch.ToString();
                }
                else
                    graph.HideFingerShape((int)XRFingerShapeType.Pinch);

                if (shapes.TryGetSpread(out var spread))
                {
                    graph.SetFingerShape((int)XRFingerShapeType.Spread, spread);
                    curlValues += ", 4:" + spread.ToString();
                }
                else
                    graph.HideFingerShape((int)XRFingerShapeType.Spread);
            }

            print(fingerIndex.ToString() + ": " +curlValues);
        }

        static XRHandSubsystem TryGetSubsystem()
        {
            SubsystemManager.GetSubsystems(s_SubsystemsReuse);
            return s_SubsystemsReuse.Count > 0 ? s_SubsystemsReuse[0] : null;
        }
    }
}
