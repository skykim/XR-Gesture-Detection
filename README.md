# XR-Gesture-Detection
XR Gesture Detection Using Unity Sentis


## Environment ##
- Unity 6000.0.23f1
- Sentis 2.1.0
- XR Hands 1.5.0

## Dataset ##
- [points_xrhands.csv](https://drive.google.com/file/d/1vN-jYTjLqpnPJgYns0o7xXuwIbshiiGM/view?usp=drive_link)
- ASL words: (I, Love, Unity, Six) from [HandSpeak](https://www.handspeak.com)
- 8,000 sets of 57 values for left and right hands
  - Class ID (1)
  - Left Hand (28) / Right Hand (28)
    - Palm Rotation (3): Euler Angle of (x,y,z)
    - Thumb (5): FullCurl, BaseCurl, TipCurl, Pinch, Spread 
    - Index (5): FullCurl, BaseCurl, TipCurl, Pinch, Spread
    - Middle (5): FullCurl, BaseCurl, TipCurl, Pinch, Spread
    - Ring (5): FullCurl, BaseCurl, TipCurl, Pinch, Spread
    - Little (5): FullCurl, BaseCurl, TipCurl, Pinch, Spread
   
## Training (Colab) ##
<a href="https://colab.research.google.com/drive/1sVIXASCz_UtOH-iNH-ztBYlsfsL69GB7?usp=sharing"><img alt="colab link" src="https://colab.research.google.com/assets/colab-badge.svg" /></a>
