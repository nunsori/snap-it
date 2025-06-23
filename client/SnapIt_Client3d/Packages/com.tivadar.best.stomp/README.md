# Best STOMP

Welcome to the Best STOMP Documentation!
Best STOMP is a top-tier Unity networking library designed for easy and efficient integration of the [STOMP protocol](https://stomp.github.io/). 
Perfect for applications requiring real-time messaging, such as chat systems, live updates, and interactive collaboration tools.

!!! Warning "**Dependency Alert**"
    Best STOMP relies on the **Best HTTP** and **Best WebSockets** packages! 
    Ensure these packages are installed and configured in your Unity project before using Best STOMP. 
    Learn more about the installation of Best HTTP and Best WebSockets.

!!! Warning
    Note that Best STOMP is a **client-side implementation**. For server-side functionalities, you'll need server-specific libraries and tools according to your programming language and platform.
    
    Ensure you're implementing the appropriate components for your project needs!

## Overview
In an era where instant and real-time communication is crucial, efficient messaging protocols are indispensable. 

**STOMP**, or Simple Text Oriented Messaging Protocol, is a straightforward, text-based messaging protocol for use with message brokers. 
It's versatile and language-agnostic, making it ideal for diverse applications, including chat systems, live updates, and collaborative environments.

Best STOMP brings the power and simplicity of the STOMP protocol into your Unity projects with ease. 
It focuses on providing robust messaging capabilities while maintaining a lightweight footprint, ensuring efficient real-time communication.

## Key Features
- **Unity Compatibility:** Works seamlessly with Unity versions :fontawesome-brands-unity: **2021.1 and newer**.
- **STOMP Protocol Support:** Fully compatible with the latest STOMP v1.2 protocol, offering a versatile messaging solution.
- **Cross-Platform Compatibility:** Operates smoothly across a spectrum of Unity-supported platforms, making it versatile for a wide range of projects. Specifically, it supports:
    - :fontawesome-solid-desktop: **Desktop:** Windows, Linux, MacOS
    - :fontawesome-solid-mobile:  **Mobile:** iOS, Android
    - :material-microsoft-windows: **Universal Windows Platform (UWP)**
    - :material-web: **Web Browsers:** WebGL
	  - Furthermore, user reports suggest that Best STOMP also functions on the following platforms. However, due to the lack of testing capabilities, official support for these platforms is not provided:
		  - :fontawesome-brands-xbox: Xbox
		  - :fontawesome-brands-playstation: PlayStation
		  - :simple-nintendoswitch: Nintendo Switch
		
		  Please note that while there is evidence of compatibility with these platforms, I'm unable to offer official support or guarantee full functionality due to testing limitations.

    Broad platform support makes Best STOMP ideal for diverse project requirements.

- **Easy Integration:** User-friendly APIs and detailed documentation simplify integration into any Unity project.
- **Optimized Performance:** Designed for high-speed, low-latency messaging, crucial for real-time applications.
- **Flexible Messaging:** Supports both text and binary-based messaging with options for custom headers and content types.
- **TCP and WebSocket Transport Support:** Offers flexible connectivity options with support for both TCP and WebSocket transports, catering to diverse network requirements and scenarios.
- **TLS for All Transports:** Ensures secure communication by enabling TLS encryption for both TCP and WebSocket connections, safeguarding data integrity and privacy.
- **Async-Await and Event-Driven Architecture:** Harness the power of C#'s async-await pattern for asynchronous operations, alongside event-based communication, to build dynamic and responsive applications.
- **Support for STOMP's Transactions:** Implement transactional messaging as defined in the STOMP protocol, providing reliable and consistent message handling and exchange.
- **Versatile Acknowledgment Modes:** Supports **'auto'**, **'client'**, and **'client-individual'** acknowledgment modes, providing flexibility in message acknowledgment strategies to suit various application needs.
- **Advanced Subscription Handling:** Manage topic subscriptions with ease, including support for multiple destination subscriptions.
- **Comprehensive Profiler Integration:** Utilize the [Best HTTP profiler](../Shared/profiler/index.md) for in-depth analysis of memory and network performance.
- **Customization Options:** Extensive configuration settings to tailor Best STOMP to your project's needs.
- **Debugging and Logging Tools:** Advanced logging capabilities aid in development and troubleshooting.

### Installing from the Unity Asset Store using the Package Manager Window

1. **Purchase:** If you haven't previously purchased the package, proceed to do so. 
    Once purchased, Unity will recognize your purchase, and you can install the package directly from within the Unity Editor. If you already own the package, you can skip these steps.
    1. **Visit the Unity Asset Store:** Navigate to the [Unity Asset Store](https://assetstore.unity.com/publishers/4137?aid=1101lfX8E) using your web browser.
    2. **Search for Best MQTT:** Locate and choose the official Best MQTT package.
    3. **Buy Best MQTT:** By clicking on the `Buy Now` button go though the purchase process.
2. **Open Unity & Access the Package Manager:** Start Unity and select your project. Head to [Window > Package Manager](https://docs.unity3d.com/Manual/upm-ui.html).
3. **Select 'My Assets':** In the Package Manager, switch to the [My Assets](https://docs.unity3d.com/Manual/upm-ui-import.html) tab to view all accessible assets.
4. **Find Best MQTT and Download:** Scroll to find "Best MQTT". Click to view its details. If it isn't downloaded, you'll notice a Download button. Click and wait. After downloading, this button will change to Import.
5. **Import the Package:** Once downloaded, click the Import button. Unity will display all Best MQTT' assets. Ensure all are selected and click Import.
6. **Confirmation:** After the import, Best MQTT will integrate into your project, signaling a successful installation.

### Installing from a .unitypackage file

If you have a .unitypackage file for Best MQTT, follow these steps:

1. **Download the .unitypackage:** Make sure the Best MQTT.unitypackage file is saved on your device. 
2. **Import into Unity:** Open Unity and your project. Go to Assets > Import Package > Custom Package.
3. **Locate and Select the .unitypackage:** Find where you saved the Best MQTT.unitypackage file, select it, and click Open.
4. **Review and Import:** Unity will show a list of all the package's assets. Ensure all assets are selected and click Import.
5. **Confirmation:** Post import, you'll see all the Best MQTT assets in your project's Asset folder, indicating a successful setup.

!!! Note
    Best MQTT also supports other installation techniques as documented in Unity's manual for packages. 
    For more advanced installation methods, please see the Unity Manual on [Sharing Packages](https://docs.unity3d.com/Manual/cus-share.html).

### Assembly Definitions and Runtime References
For developers familiar with Unity's development patterns, it's essential to understand how Best MQTT incorporates Unity's systems:

- **Assembly Definition Files:** Best MQTT incorporates [Unity's Assembly Definition files](https://docs.unity3d.com/Manual/ScriptCompilationAssemblyDefinitionFiles.html). It aids in organizing and managing the codebase efficiently.
- **Auto-Referencing of Runtime DLLs:** The runtime DLLs produced by Best MQTT are [Auto Referenced](https://docs.unity3d.com/Manual/class-AssemblyDefinitionImporter.html), allowing Unity to automatically recognize and utilize them without manual intervention.
- **Manual Package Referencing:** Should you need to reference Best MQTT manually in your project (for advanced setups or specific use cases), you can do so. 
Simply [reference the package](https://docs.unity3d.com/Manual/ScriptCompilationAssemblyDefinitionFiles.html#reference-another-assembly) by searching for `com.Tivadar.Best.MQTT`.

Congratulations! You've successfully integrated Best MQTT into your Unity project. Begin your MQTT adventure with the Getting Started guide.

For any issues or additional assistance, please consult the Community and Support page.