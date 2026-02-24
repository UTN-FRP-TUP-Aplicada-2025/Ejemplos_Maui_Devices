         
# Errores en compilación


## pipeline:
```log

BUILD SIMULADOR. Construye para el simulador iOS
         Run cd /Users/runner/work/Ejemplos_Maui_Devices/Ejemplos_Maui_Devices/Ejemplos_Devices
  cd /Users/runner/work/Ejemplos_Maui_Devices/Ejemplos_Maui_Devices/Ejemplos_Devices
  
  echo "Proyecto: Ejemplo_LectorQR_Dialog.csproj"
  echo "Build: Release"
  echo "Target framework: net10.0-ios"
  
  dotnet build "QR/Ejemplo_LectorQR_Dialog/Ejemplo_LectorQR_Dialog.csproj" \
      -c Release \
      -f:net10.0-ios \
      -p:RuntimeIdentifier=iossimulator-arm64 \
      -p:CodesignEntitlements=Platforms/iOS/Entitlements.Development.plist \
      -p:EnableCodeSigning=false \
      -p:CodesignProvision="" \
      -p:CodesignKey="-" \
      -v normal
  
  echo "Imprime la carpeta bin recursiva:"
  ls -Rl "QR/Ejemplo_LectorQR_Dialog/Ejemplo_LectorQR_Dialog.csproj/bin/"
  
  BASE_PATH=/Users/runner/work/Ejemplos_Maui_Devices/Ejemplos_Maui_Devices/Ejemplos_Devices/QR/Ejemplo_LectorQR_Dialog/bin/Release/net10.0-ios/iossimulator-arm64
  APP_PATH=${BASE_PATH}/QR/Ejemplo_LectorQR_Dialog.app
  
  echo "BASE_PATH: $BASE_PATH"      
  echo "APP_PATH: $APP_PATH"
  
  echo "BASE_PATH=$BASE_PATH" >> $GITHUB_ENV
  echo "APP_PATH=$APP_PATH" >> $GITHUB_ENV
     
  shell: /bin/bash -e {0}
  env:
    XCODE_VERSION: 26.0
    XCODE_VERSION_SHORT: 26.0
    XCODE_FILE_INSTALLER: Xcode_26_Universal.xip
    XCODE_GOOGLE_FILE_INSTALLER_ID: 1GoDmCUBOMKM5nnXCXxxnAgoKpSwZvaMP
    DOTNET_VERSION: 10.0.102
    DOTNET_TARGET_VERSION: net10.0
    DOTNET_VERSION_WORKLOAD: 10.0.100
    PACKAGE_NAME: com.ejemplos.devices.qr.dialog
    SOLUTION_FOLDER: Ejemplos_Devices
    PROJECT_FOLDER: QR/Ejemplo_LectorQR_Dialog
    PROJECT_FILE: Ejemplo_LectorQR_Dialog.csproj
    VERSION_FECHA: 20260202
    RUNTIME_IDENTIFIER_SIMULATOR: iossimulator-arm64
    BUILD_CONFIG_SIMULATOR: Release
    DEVICE_SIMULATOR: iPhone 17 Pro Max
    DOTNET_WORKLOAD_INSTALL_ADDITIONAL_ARGS: --ios-simulator-runtime=26.0
    MD_APPLE_SDK_ROOT: /Applications/Xcode_26.0.app
    PROJECT_PATH: /Users/runner/work/Ejemplos_Maui_Devices/Ejemplos_Maui_Devices/Ejemplos_Devices/QR/Ejemplo_LectorQR_Dialog
    SOLUTION_PATH: /Users/runner/work/Ejemplos_Maui_Devices/Ejemplos_Maui_Devices/Ejemplos_Devices
    APP_VERSION_BUILD: 1.0_1.0
    XCODE_DEVELOPER_DIR_PATH: /Applications/Xcode_26.0.app/Contents/Developer
Proyecto: Ejemplo_LectorQR_Dialog.csproj
Build: Release
Target framework: net10.0-ios
Build started 2/24/2026 3:29:34 AM.
     1>Project "/Users/runner/work/Ejemplos_Maui_Devices/Ejemplos_Maui_Devices/Ejemplos_Devices/QR/Ejemplo_LectorQR_Dialog/Ejemplo_LectorQR_Dialog.csproj" on node 1 (Restore target(s)).
     1>_GetAllRestoreProjectPathItems:
         Determining projects to restore...
     1>Project "/Users/runner/work/Ejemplos_Maui_Devices/Ejemplos_Maui_Devices/Ejemplos_Devices/QR/Ejemplo_LectorQR_Dialog/Ejemplo_LectorQR_Dialog.csproj" (1) is building "/Users/runner/work/Ejemplos_Maui_Devices/Ejemplos_Maui_Devices/Ejemplos_Devices/QR/Ejemplo_LectorQR_Dialog/Ejemplo_LectorQR_Dialog.csproj" (1:10) on node 1 (_GenerateProjectRestoreGraph target(s)).
  1:10>Project "/Users/runner/work/Ejemplos_Maui_Devices/Ejemplos_Maui_Devices/Ejemplos_Devices/QR/Ejemplo_LectorQR_Dialog/Ejemplo_LectorQR_Dialog.csproj" (1:10) is building "/Users/runner/work/Ejemplos_Maui_Devices/Ejemplos_Maui_Devices/Ejemplos_Devices/QR/Ejemplo_LectorQR_Dialog/Ejemplo_LectorQR_Dialog.csproj" (1:13) on node 1 (_GenerateProjectRestoreGraphPerFramework target(s)).
     1>ProcessFrameworkReferences:
       /Users/runner/.dotnet/sdk/10.0.102/Sdks/Microsoft.NET.Sdk/targets/Microsoft.NET.Sdk.FrameworkReferenceResolution.targets(190,5): message NETSDK1084: There is no application host available for the specified RuntimeIdentifier 'android-arm'. [/Users/runner/work/Ejemplos_Maui_Devices/Ejemplos_Maui_Devices/Ejemplos_Devices/QR/Ejemplo_LectorQR_Dialog/Ejemplo_LectorQR_Dialog.csproj::TargetFramework=net10.0-android]
/Users/runner/.dotnet/sdk/10.0.102/Sdks/Microsoft.NET.Sdk/targets/Microsoft.NET.Sdk.FrameworkReferenceResolution.targets(190,5): message NETSDK1084: There is no application host available for the specified RuntimeIdentifier 'android-arm64'. [/Users/runner/work/Ejemplos_Maui_Devices/Ejemplos_Maui_Devices/Ejemplos_Devices/QR/Ejemplo_LectorQR_Dialog/Ejemplo_LectorQR_Dialog.csproj::TargetFramework=net10.0-android]
       /Users/runner/.dotnet/sdk/10.0.102/Sdks/Microsoft.NET.Sdk/targets/Microsoft.NET.Sdk.FrameworkReferenceResolution.targets(190,5): message NETSDK1084: There is no application host available for the specified RuntimeIdentifier 'android-x86'. [/Users/runner/work/Ejemplos_Maui_Devices/Ejemplos_Maui_Devices/Ejemplos_Devices/QR/Ejemplo_LectorQR_Dialog/Ejemplo_LectorQR_Dialog.csproj::TargetFramework=net10.0-android]
       /Users/runner/.dotnet/sdk/10.0.102/Sdks/Microsoft.NET.Sdk/targets/Microsoft.NET.Sdk.FrameworkReferenceResolution.targets(190,5): message NETSDK1084: There is no application host available for the specified RuntimeIdentifier 'android-x64'. [/Users/runner/work/Ejemplos_Maui_Devices/Ejemplos_Maui_Devices/Ejemplos_Devices/QR/Ejemplo_LectorQR_Dialog/Ejemplo_LectorQR_Dialog.csproj::TargetFramework=net10.0-android]
       AddPrunePackageReferences:
         Loading prune package data from PrunePackageData folder
         Failed to load prune package data from PrunePackageData folder, loading from targeting packs instead
         Looking for targeting packs in /Users/runner/.dotnet/packs/Microsoft.NETCore.App.Ref
         Pack directories found: /Users/runner/.dotnet/packs/Microsoft.NETCore.App.Ref/10.0.2
         Found package overrides file /Users/runner/.dotnet/packs/Microsoft.NETCore.App.Ref/10.0.2/data/PackageOverrides.txt
     1>Done Building Project "/Users/runner/work/Ejemplos_Maui_Devices/Ejemplos_Maui_Devices/Ejemplos_Devices/QR/Ejemplo_LectorQR_Dialog/Ejemplo_LectorQR_Dialog.csproj" (_GenerateProjectRestoreGraphPerFramework target(s)).
  1:10>Project "/Users/runner/work/Ejemplos_Maui_Devices/Ejemplos_Maui_Devices/Ejemplos_Devices/QR/Ejemplo_LectorQR_Dialog/Ejemplo_LectorQR_Dialog.csproj" (1:10) is building "/Users/runner/work/Ejemplos_Maui_Devices/Ejemplos_Maui_Devices/Ejemplos_Devices/QR/Ejemplo_LectorQR_Dialog/Ejemplo_LectorQR_Dialog.csproj" (1:14) on node 1 (_GenerateProjectRestoreGraphPerFramework target(s)).
     1>AddPrunePackageReferences:
         Loading prune package data from PrunePackageData folder
         Failed to load prune package data from PrunePackageData folder, loading from targeting packs instead
```
...
         
```         
         Started external tool execution #18: xcrun clang++ -F /Users/runner/Library/Caches/XamarinBuildDownload/MLKCommon-12.0.0/Frameworks -framework MLKitCommon -F /Users/runner/work/Ejemplos_Maui_Devices/Ejemplos_Maui_Devices/Ejemplos_Devices/QR/Ejemplo_LectorQR_Dialog -framework Foundation -F /Users/runner/work/Ejemplos_Maui_Devices/Ejemplos_Maui_Devices/Ejemplos_Devices/QR/Ejemplo_LectorQR_Dialog -framework LocalAuthentication -F /Users/runner/Library/Caches/XamarinBuildDownload/MLKVision-8.0.0/Frameworks -framework MLKitVision -F /Users/runner/work/Ejemplos_Maui_Devices/Ejemplos_Maui_Devices/Ejemplos_Devices/QR/Ejemplo_LectorQR_Dialog -framework AVFoundation -F /Users/runner/work/Ejemplos_Maui_Devices/Ejemplos_Maui_Devices/Ejemplos_Devices/QR/Ejemplo_LectorQR_Dialog -framework Accelerate -F /Users/runner/work/Ejemplos_Maui_Devices/Ejemplos_Maui_Devices/Ejemplos_Devices/QR/Ejemplo_LectorQR_Dialog -framework CoreGraphics -F /Users/runner/work/Ejemplos_Maui_Devices/Ejemplos_Maui_Devices/Ejemplos_Devices/QR/Ejemplo_LectorQR_Dialog -framework CoreMedia -F /Users/runner/work/Ejemplos_Maui_Devices/Ejemplos_Maui_Devices/Ejemplos_Devices/QR/Ejemplo_LectorQR_Dialog -framework CoreVideo -F /Users/runner/work/Ejemplos_Maui_Devices/Ejemplos_Maui_Devices/Ejemplos_Devices/QR/Ejemplo_LectorQR_Dialog -framework UIKit -F /Users/runner/Library/Caches/XamarinBuildDownload/MLImage-1.0.0-beta6/Frameworks -framework MLImage -F /Users/runner/Library/Caches/XamarinBuildDownload/MLKitBarcodeScanning-6.0.0/Frameworks -framework MLKitBarcodeScanning -F /Users/runner/work/Ejemplos_Maui_Devices/Ejemplos_Maui_Devices/Ejemplos_Devices/QR/Ejemplo_LectorQR_Dialog -framework CoreImage -F /Users/runner/.nuget/packages/adame.google.ios.googledatatransport/10.1.0/lib/net6.0-ios16.1/Google.GoogleDataTransport.resources/GoogleDataTransport.xcframework/ios-arm64_x86_64-simulator -framework GoogleDataTransport -F /Users/runner/work/Ejemplos_Maui_Devices/Ejemplos_Maui_Devices/Ejemplos_Devices/QR/Ejemplo_LectorQR_Dialog -framework SystemConfiguration -F /Users/runner/work/Ejemplos_Maui_Devices/Ejemplos_Maui_Devices/Ejemplos_Devices/QR/Ejemplo_LectorQR_Dialog -framework CoreTelephony -F /Users/runner/.nuget/packages/adame.google.ios.gtmsessionfetcher/3.5.0/lib/net6.0-ios16.1/Google.GTMSessionFetcher.resources/GTMSessionFetcher.xcframework/ios-arm64_x86_64-simulator -framework GTMSessionFetcher -F /Users/runner/.nuget/packages/adame.google.ios.nanopb/3.30910.0/lib/net6.0-ios16.1/Google.Nanopb.resources/nanopb.xcframework/ios-arm64_x86_64-simulator -framework nanopb -F /Users/runner/.nuget/packages/adame.google.ios.promisesobjc/2.4.0/lib/net6.0-ios16.1/Google.PromisesObjC.resources/FBLPromises.xcframework/ios-arm64_x86_64-simulator -framework FBLPromises -F /Users/runner/.nuget/packages/adame.mlkit.ios.core/12.0.0.2/lib/net9.0-ios18.0/MLKit.Core.resources/GoogleToolboxForMac.xcframework/ios-arm64_x86_64-simulator -framework GoogleToolboxForMac -lc++ -lsqlite3 -lz -lc++ -lc++ -lsqlite3 -lz -lc++ -lz -Lobj/Release/net10.0-ios/iossimulator-arm64/nativelibraries -lSystem.Globalization.Native -lSystem.IO.Compression.Native -lSystem.Native -lSystem.Net.Security.Native -lSystem.Security.Cryptography.Native.Apple -lmono-component-debugger -lmono-component-diagnostics_tracing -lmono-component-hot_reload -lmono-component-marshal-ilgen -lmonosgen-2.0 -lxamarin-dotnet -miphonesimulator-version-min=15.0 -isysroot /Applications/Xcode_26.0.app/Contents/Developer/Platforms/iPhoneSimulator.platform/Developer/SDKs/iPhoneSimulator26.0.sdk -arch arm64 /Users/runner/work/Ejemplos_Maui_Devices/Ejemplos_Maui_Devices/Ejemplos_Devices/QR/Ejemplo_LectorQR_Dialog/obj/Release/net10.0-ios/iossimulator-arm64/nativelibraries/aot-output/arm64/System.Private.CoreLib.dll.o /Users/runner/work/Ejemplos_Maui_Devices/Ejemplos_Maui_Devices/Ejemplos_Devices/QR/Ejemplo_LectorQR_Dialog/obj/Release/net10.0-ios/iossimulator-arm64/nativelibraries/aot-output/arm64/System.Private.CoreLib.dll.llvm.o -L/Users/runner/.dotnet/packs/Microsoft.iOS.Runtime.iossimulator-arm64.net10.0_26.0/26.0.11017/runtimes/iossimulator-arm64/native -lxamarin-dotnet -L/Users/runner/.dotnet/packs/Microsoft.NETCore.App.Runtime.Mono.iossimulator-arm64/10.0.0/runtimes/iossimulator-arm64/native -lSystem.Globalization.Native -L/Users/runner/.dotnet/packs/Microsoft.NETCore.App.Runtime.Mono.iossimulator-arm64/10.0.0/runtimes/iossimulator-arm64/native -lSystem.IO.Compression.Native -L/Users/runner/.dotnet/packs/Microsoft.NETCore.App.Runtime.Mono.iossimulator-arm64/10.0.0/runtimes/iossimulator-arm64/native -lSystem.Native -L/Users/runner/.dotnet/packs/Microsoft.NETCore.App.Runtime.Mono.iossimulator-arm64/10.0.0/runtimes/iossimulator-arm64/native -lSystem.Net.Security.Native -L/Users/runner/.dotnet/packs/Microsoft.NETCore.App.Runtime.Mono.iossimulator-arm64/10.0.0/runtimes/iossimulator-arm64/native -lSystem.Security.Cryptography.Native.Apple -L/Users/runner/.dotnet/packs/Microsoft.NETCore.App.Runtime.Mono.iossimulator-arm64/10.0.0/runtimes/iossimulator-arm64/native -lmono-component-debugger -L/Users/runner/.dotnet/packs/Microsoft.NETCore.App.Runtime.Mono.iossimulator-arm64/10.0.0/runtimes/iossimulator-arm64/native -lmono-component-diagnostics_tracing -L/Users/runner/.dotnet/packs/Microsoft.NETCore.App.Runtime.Mono.iossimulator-arm64/10.0.0/runtimes/iossimulator-arm64/native -lmono-component-hot_reload -L/Users/runner/.dotnet/packs/Microsoft.NETCore.App.Runtime.Mono.iossimulator-arm64/10.0.0/runtimes/iossimulator-arm64/native -lmono-component-marshal-ilgen -L/Users/runner/.dotnet/packs/Microsoft.NETCore.App.Runtime.Mono.iossimulator-arm64/10.0.0/runtimes/iossimulator-arm64/native -lmonosgen-2.0 -rpath @executable_path -framework AudioToolbox -framework AuthenticationServices -framework AVFoundation -framework CloudKit -framework Contacts -framework ContactsUI -framework CoreData -framework CoreFoundation -framework CoreGraphics -framework CoreImage -framework CoreLocation -framework CoreMedia -framework CoreText -framework CoreVideo -framework Foundation -framework ImageIO -framework Intents -framework LinkPresentation -framework MediaPlayer -framework PhotosUI -framework QuartzCore -framework QuickLook -framework SafariServices -framework SceneKit -framework Security -framework UIKit -framework Vision -framework WebKit -framework GSS -framework CFNetwork -framework Security -rpath @executable_path/Frameworks /Users/runner/work/Ejemplos_Maui_Devices/Ejemplos_Maui_Devices/Ejemplos_Devices/QR/Ejemplo_LectorQR_Dialog/obj/Release/net10.0-ios/iossimulator-arm64/nativelibraries/main.arm64.o -Xlinker -sectcreate -Xlinker __TEXT -Xlinker __entitlements -Xlinker /Users/runner/work/Ejemplos_Maui_Devices/Ejemplos_Maui_Devices/Ejemplos_Devices/QR/Ejemplo_LectorQR_Dialog/obj/Release/net10.0-ios/iossimulator-arm64/Entitlements-Simulated.xcent -Xlinker -sectcreate -Xlinker __TEXT -Xlinker __ents_der -Xlinker /Users/runner/work/Ejemplos_Maui_Devices/Ejemplos_Maui_Devices/Ejemplos_Devices/QR/Ejemplo_LectorQR_Dialog/obj/Release/net10.0-ios/iossimulator-arm64/Entitlements-Simulated.xcent.der -o /Users/runner/work/Ejemplos_Maui_Devices/Ejemplos_Maui_Devices/Ejemplos_Devices/QR/Ejemplo_LectorQR_Dialog/obj/Release/net10.0-ios/iossimulator-arm64/nativelibraries/Ejemplo_LectorQR_Dialog -dead_strip -L/Applications/Xcode_26.0.app/Contents/Developer/Toolchains/XcodeDefault.xctoolchain/usr/lib/swift/iphonesimulator/ -L/Applications/Xcode_26.0.app/Contents/Developer/Platforms/iPhoneSimulator.platform/Developer/SDKs/iPhoneSimulator26.0.sdk/usr/lib/swift/ -lz -liconv -lcompression -licucore -lobjc -exported_symbols_list obj/Release/net10.0-ios/iossimulator-arm64/mtouch-symbols.list
         Finished external tool execution #18 in 00:00:03.5060080 and with exit code 1.
         ld: warning: ignoring duplicate libraries: '-lSystem.Globalization.Native', '-lSystem.IO.Compression.Native', '-lSystem.Native', '-lSystem.Net.Security.Native', '-lSystem.Security.Cryptography.Native.Apple', '-lc++', '-lmono-component-debugger', '-lmono-component-diagnostics_tracing', '-lmono-component-hot_reload', '-lmono-component-marshal-ilgen', '-lmonosgen-2.0', '-lsqlite3', '-lxamarin-dotnet', '-lz'
         ld: building for 'iOS-simulator', but linking in object file (/Users/runner/Library/Caches/XamarinBuildDownload/MLKCommon-12.0.0/Frameworks/MLKitCommon.framework/MLKitCommon[arm64][2](MLKAnalyticsLogger.o)) built for 'iOS'
         clang++: error: linker command failed with exit code 1 (use -v to see invocation)
         
     1>/Users/runner/.dotnet/packs/Microsoft.iOS.Sdk.net10.0_26.0/26.0.11017/targets/Xamarin.Shared.Sdk.targets(1873,3): error : clang++ exited with code 1: [/Users/runner/work/Ejemplos_Maui_Devices/Ejemplos_Maui_Devices/Ejemplos_Devices/QR/Ejemplo_LectorQR_Dialog/Ejemplo_LectorQR_Dialog.csproj::TargetFramework=net10.0-ios]
/Users/runner/.dotnet/packs/Microsoft.iOS.Sdk.net10.0_26.0/26.0.11017/targets/Xamarin.Shared.Sdk.targets(1873,3): error : ld: building for 'iOS-simulator', but linking in object file (/Users/runner/Library/Caches/XamarinBuildDownload/MLKCommon-12.0.0/Frameworks/MLKitCommon.framework/MLKitCommon[arm64][2](MLKAnalyticsLogger.o)) built for 'iOS' [/Users/runner/work/Ejemplos_Maui_Devices/Ejemplos_Maui_Devices/Ejemplos_Devices/QR/Ejemplo_LectorQR_Dialog/Ejemplo_LectorQR_Dialog.csproj::TargetFramework=net10.0-ios]
/Users/runner/.dotnet/packs/Microsoft.iOS.Sdk.net10.0_26.0/26.0.11017/targets/Xamarin.Shared.Sdk.targets(1873,3): error : clang++: error: linker command failed with exit code 1 (use -v to see invocation) [/Users/runner/work/Ejemplos_Maui_Devices/Ejemplos_Maui_Devices/Ejemplos_Devices/QR/Ejemplo_LectorQR_Dialog/Ejemplo_LectorQR_Dialog.csproj::TargetFramework=net10.0-ios]
     1>Done Building Project "/Users/runner/work/Ejemplos_Maui_Devices/Ejemplos_Maui_Devices/Ejemplos_Devices/QR/Ejemplo_LectorQR_Dialog/Ejemplo_LectorQR_Dialog.csproj" (default targets) -- FAILED.
```

## Resumen

```
There is no application host available for the specified RuntimeIdentifier 'android-arm'. 
message NETSDK1084: There is no application host available for the specified RuntimeIdentifier 'android-arm64'. 
 ld: building for 'iOS-simulator', but linking in object file (/Users/runner/Library/Caches/XamarinBuildDownload/MLKCommon-12.0.0/Frameworks/MLKitCommon.framework/MLKitCommon[arm64][2](MLKAnalyticsLogger.o)) built for 'iOS'
         clang++: error: linker command failed with exit code 1 (use -v to see invocation)
```


## Archivo de proyecto
```xml

<Project Sdk="Microsoft.NET.Sdk">

	<PropertyGroup>
		<TargetFrameworks>net10.0-android</TargetFrameworks>
		<TargetFrameworks Condition="$([MSBuild]::IsOSPlatform('osx'))">$(TargetFrameworks);net10.0-ios</TargetFrameworks>

		<OutputType>Exe</OutputType>
		
		<RootNamespace>Ejemplo_LectorQR_Dialog</RootNamespace>
		
		<UseMaui>true</UseMaui>
		<SingleProject>true</SingleProject>
		<ImplicitUsings>enable</ImplicitUsings>
		<Nullable>enable</Nullable>
		
		<ApplicationTitle>QR Lector</ApplicationTitle>
		<ApplicationId>com.ejemplos.devices.qr.dialog</ApplicationId>
		
		<ApplicationDisplayVersion>1.0</ApplicationDisplayVersion>
		<ApplicationVersion>1</ApplicationVersion>
		
		<WindowsPackageType>None</WindowsPackageType>
		
		<SupportedOSPlatformVersion Condition="$([MSBuild]::GetTargetPlatformIdentifier('$(TargetFramework)')) == 'ios'">15.0</SupportedOSPlatformVersion>
		<SupportedOSPlatformVersion Condition="$([MSBuild]::GetTargetPlatformIdentifier('$(TargetFramework)')) == 'android'">25.0</SupportedOSPlatformVersion>
	</PropertyGroup>

	<!--
	<PropertyGroup Condition="$([MSBuild]::GetTargetPlatformIdentifier('$(TargetFramework)')) == 'android'">
		<AndroidLdFlags>-Wl,-z,max-page-size=16384</AndroidLdFlags>
		<AndroidUseAapt2>true</AndroidUseAapt2>
		<RuntimeIdentifiers>android-arm;android-arm64;android-x86;android-x64</RuntimeIdentifiers>
		<InvariantGlobalization>false</InvariantGlobalization>
	</PropertyGroup>
	-->

	<PropertyGroup Condition="$([MSBuild]::GetTargetPlatformIdentifier('$(TargetFramework)')) == 'android'">
		<AndroidLdFlags>-Wl,-z,max-page-size=16384</AndroidLdFlags>
		<RuntimeIdentifiers>android-arm;android-arm64;android-x86;android-x64</RuntimeIdentifiers>
	</PropertyGroup>

	<PropertyGroup Condition="$(TargetFramework.Contains('-ios')) and '$(Configuration)' == 'Release'">
		<UseInterpreter>true</UseInterpreter>
		<PublishTrimmed>true</PublishTrimmed>

		<MtouchLink>SdkOnly</MtouchLink>
		<MtouchExtraArgs>--linkskip=System.Net.Mail --linkskip=System.Net.Ping</MtouchExtraArgs>
		<EmbedAssembliesIntoApk>true</EmbedAssembliesIntoApk>

		<RunAOTCompilation Condition="'$(RuntimeIdentifier)' == 'ios-arm64'">true</RunAOTCompilation>
		<RunAOTCompilation Condition="'$(RuntimeIdentifier)' != 'ios-arm64'">false</RunAOTCompilation>
	</PropertyGroup>

	
	<PropertyGroup Condition="$(TargetFramework.Contains('-ios')) AND '$(RuntimeIdentifier)' == 'iossimulator-arm64'">
		<MtouchExtraArgs>$(MtouchExtraArgs) --setenv:DYLD_LIBRARY_PATH=/usr/lib/system/introspection</MtouchExtraArgs>
		<NoWarn>$(NoWarn);MT5209;MT5212;MT5213</NoWarn>
	</PropertyGroup>

	<ItemGroup>
		<MauiIcon Include="Resources\AppIcon\appicon.svg" ForegroundFile="Resources\AppIcon\appiconfg.svg" Color="#512BD4" />
		<MauiSplashScreen Include="Resources\Splash\splash.svg" Color="#512BD4" BaseSize="128,128" />
		<MauiImage Include="Resources\Images\*" />
		<MauiImage Update="Resources\Images\dotnet_bot.png" Resize="True" BaseSize="300,185" />		
		<MauiFont Include="Resources\Fonts\*" />
		<MauiAsset Include="Resources\Raw\**" LogicalName="%(RecursiveDir)%(Filename)%(Extension)" />
	</ItemGroup>

	<ItemGroup>
	  <None Remove="Platforms\Android\Resources\values\styles.xml" />
	</ItemGroup>

	<ItemGroup>
		<PackageReference Include="BarcodeScanner.Mobile.Maui" Version="9.0.1" />
		<PackageReference Include="Microsoft.Maui.Controls" Version="$(MauiVersion)" />
		<PackageReference Include="Microsoft.Extensions.Logging.Debug" Version="10.0.2" />
	</ItemGroup>

	<ItemGroup>
	  <Compile Update="Pages\QRLectorPage.xaml.cs">
	    <DependentUpon>QRLectorPage.xaml</DependentUpon>
	  </Compile>
	</ItemGroup>

</Project>
```


<!--
actual
	<PropertyGroup Condition="$(TargetFramework.Contains('-ios')) AND '$(RuntimeIdentifier)' == 'iossimulator-arm64'">
		<MtouchExtraArgs>$(MtouchExtraArgs) --setenv:DYLD_LIBRARY_PATH=/usr/lib/system/introspection</MtouchExtraArgs>
		<NoWarn>$(NoWarn);MT5209;MT5212;MT5213</NoWarn>
	</PropertyGroup>

	<PropertyGroup Condition="$(TargetFramework.Contains('-ios')) AND '$(RuntimeIdentifier)' == 'iossimulator-arm64'">
		<MtouchExtraArgs>$(MtouchExtraArgs) -gcc_flags "-Wl,-ignore_auto_link -Wl,-read_only_relocs,suppress"</MtouchExtraArgs>
		<MtouchLink>None</MtouchLink>
	</PropertyGroup>
-->

solucion

<PropertyGroup Condition="$(TargetFramework.Contains('-ios')) AND '$(RuntimeIdentifier)' == 'iossimulator-arm64'">
    <MtouchExtraArgs>$(MtouchExtraArgs) --setenv:DYLD_LIBRARY_PATH=/usr/lib/system/introspection -gcc_flags "-Wl,-ignore_auto_link -Wl,-read_only_relocs,suppress"</MtouchExtraArgs>
        <MtouchLink>SdkOnly</MtouchLink>
    <NoWarn>$(NoWarn);MT5209;MT5212;MT5213</NoWarn>
</PropertyGroup>

- name: BUILD SIMULADOR. Construye para el simulador iOS
  run: |
    cd ${{ env.SOLUTION_PATH }}
    dotnet build "${{ env.PROJECT_FOLDER }}/${{ env.PROJECT_FILE }}" \
        -c ${{ env.BUILD_CONFIG_SIMULATOR }} \
        -f net10.0-ios \
        -p:RuntimeIdentifier=${{ env.RUNTIME_IDENTIFIER_SIMULATOR }} \
        -p:LinkMode=SdkOnly \
        -p:CLI_Build=true