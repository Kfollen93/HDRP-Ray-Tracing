# HDRP Ray Tracing Template
<img src="https://github.com/Kfollen93/HDRP-Ray-Tracing/blob/main/Gif/hdrprt.gif" width="640" height="360"/>

*Template script to toggle buttons for Ray Tracing, and as an extra, DLSS Modes (Max Quality, Balanced, Max Performance) via a keypress toggle.*
## Description
While I am in the finishing stretch for a URP project that I have been working on for several months, I got distracted by Unity's recent 2022.2 Tech Stream release. I have only worked with the built-in render pipeline and the URP. However, all of the new features in 2022.2 and the HDRP in general has interested me for some time. If my next game ends up utilizing the HDRP, then I will certainly be using DX12 and Ray Tracing. Therefore, the script here is intended to be used as a resource for getting you started with toggling Ray Tracing settings in Unity's HDRP. 

## How to Use
You will need to be in a HDRP project with all of the DXR settings enabled (and DLSS if you want to use it). Once your project is set up for Ray Tracing, you can then create an empty GameObject and place the script on it. From there, it's simply a drag and drop of all the required components.
<br>
One of those required components is a Volume Profile. To utilize all of the Ray Tracing features, this Volume should include: Screen Space Global Illumination, Screen Space Reflection, and Screen Space Ambient Occlusion. An error will be thrown if one of those overrides are not included on the Volume; feel free to remove this requirement if you don't want it.<br>
<br>
<img src="https://github.com/Kfollen93/HDRP-Ray-Tracing/blob/main/Gif/Settings.PNG"/>


## Additional Information
<ul>
  <li>Made with: Unity 2022.2.1f1 (HDRP)</li> - It's likely some settings will change when 2022.3 arrives.
  <li>Hardware Used: RTX 3080.</li>
</ul>
