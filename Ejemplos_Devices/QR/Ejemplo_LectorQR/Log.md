dotnet restore -p:TargetFramework=net9.0-android

Solución

<ItemGroup Condition="'$(Configuration)|$(TargetFramework)'=='Debug|net9.0-android'">
		<PackageReference Include="Xamarin.AndroidX.Lifecycle.Runtime" Version="2.8.7.3" />
		<PackageReference Include="Xamarin.AndroidX.Lifecycle.ViewModel" Version="2.8.7.3" />
	</ItemGroup>

Compilación iniciada a las 12:03...
1>------ Operación Compilar iniciada: Proyecto: Ejemplo_LectorQR, configuración: Debug Any CPU ------
1>E:\repos\tup\aplicada\2025\utn\Ejemplos_Maui_Devices\Ejemplo_Devices\Ejemplo_LectorQR\Ejemplo_LectorQR.csproj : warning NU1608: Se detectó una versión del paquete fuera de la restricción de dependencia: Xamarin.AndroidX.Lifecycle.Runtime.Ktx 2.8.7.3 requiere Xamarin.AndroidX.Lifecycle.Runtime (>= 2.8.7.3 && < 2.8.8), pero la versión Xamarin.AndroidX.Lifecycle.Runtime 2.9.2.1 ya se resolvió.
1>E:\repos\tup\aplicada\2025\utn\Ejemplos_Maui_Devices\Ejemplo_Devices\Ejemplo_LectorQR\Ejemplo_LectorQR.csproj : warning NU1608: Se detectó una versión del paquete fuera de la restricción de dependencia: Xamarin.AndroidX.Lifecycle.ViewModel.Ktx 2.8.7.3 requiere Xamarin.AndroidX.Lifecycle.ViewModel (>= 2.8.7.3 && < 2.8.8), pero la versión Xamarin.AndroidX.Lifecycle.ViewModel 2.9.2.1 ya se resolvió.
1>E:\repos\tup\aplicada\2025\utn\Ejemplos_Maui_Devices\Ejemplo_Devices\Ejemplo_LectorQR\Ejemplo_LectorQR.csproj : warning NU1608: Se detectó una versión del paquete fuera de la restricción de dependencia: Xamarin.AndroidX.SavedState.SavedState.Ktx 1.2.1.16 requiere Xamarin.AndroidX.SavedState (>= 1.2.1.16 && < 1.2.2), pero la versión Xamarin.AndroidX.SavedState 1.3.1.1 ya se resolvió.
1>E:\repos\tup\aplicada\2025\utn\Ejemplos_Maui_Devices\Ejemplo_Devices\Ejemplo_LectorQR\Ejemplo_LectorQR.csproj : warning NU1608: Se detectó una versión del paquete fuera de la restricción de dependencia: Xamarin.AndroidX.Fragment.Ktx 1.8.6.1 requiere Xamarin.AndroidX.Fragment (>= 1.8.6.1 && < 1.8.7), pero la versión Xamarin.AndroidX.Fragment 1.8.8.1 ya se resolvió.
1>E:\repos\tup\aplicada\2025\utn\Ejemplos_Maui_Devices\Ejemplo_Devices\Ejemplo_LectorQR\Ejemplo_LectorQR.csproj : warning NU1608: Se detectó una versión del paquete fuera de la restricción de dependencia: Xamarin.AndroidX.Lifecycle.Runtime.Ktx.Android 2.8.7.3 requiere Xamarin.AndroidX.Lifecycle.Runtime (>= 2.8.7.3 && < 2.8.8), pero la versión Xamarin.AndroidX.Lifecycle.Runtime 2.9.2.1 ya se resolvió.
1>  Omitiendo analizadores para acelerar la compilación. Puede ejecutar los comandos "Build" o "Rebuild" para ejecutar los analizadores.
1>  Including assemblies for Hot Reload support
1>  Ejemplo_LectorQR -> E:\repos\tup\aplicada\2025\utn\Ejemplos_Maui_Devices\Ejemplo_Devices\Ejemplo_LectorQR\bin\Debug\net9.0-android\Ejemplo_LectorQR.dll
1>E:\repos\tup\aplicada\2025\utn\Ejemplos_Maui_Devices\Ejemplo_Devices\Ejemplo_LectorQR\Ejemplo_LectorQR.csproj : warning NU1608: Se detectó una versión del paquete fuera de la restricción de dependencia: Xamarin.AndroidX.Lifecycle.Runtime.Ktx 2.8.7.3 requiere Xamarin.AndroidX.Lifecycle.Runtime (>= 2.8.7.3 && < 2.8.8), pero la versión Xamarin.AndroidX.Lifecycle.Runtime 2.9.2.1 ya se resolvió.
1>E:\repos\tup\aplicada\2025\utn\Ejemplos_Maui_Devices\Ejemplo_Devices\Ejemplo_LectorQR\Ejemplo_LectorQR.csproj : warning NU1608: Se detectó una versión del paquete fuera de la restricción de dependencia: Xamarin.AndroidX.Lifecycle.ViewModel.Ktx 2.8.7.3 requiere Xamarin.AndroidX.Lifecycle.ViewModel (>= 2.8.7.3 && < 2.8.8), pero la versión Xamarin.AndroidX.Lifecycle.ViewModel 2.9.2.1 ya se resolvió.
1>E:\repos\tup\aplicada\2025\utn\Ejemplos_Maui_Devices\Ejemplo_Devices\Ejemplo_LectorQR\Ejemplo_LectorQR.csproj : warning NU1608: Se detectó una versión del paquete fuera de la restricción de dependencia: Xamarin.AndroidX.SavedState.SavedState.Ktx 1.2.1.16 requiere Xamarin.AndroidX.SavedState (>= 1.2.1.16 && < 1.2.2), pero la versión Xamarin.AndroidX.SavedState 1.3.1.1 ya se resolvió.
1>E:\repos\tup\aplicada\2025\utn\Ejemplos_Maui_Devices\Ejemplo_Devices\Ejemplo_LectorQR\Ejemplo_LectorQR.csproj : warning NU1608: Se detectó una versión del paquete fuera de la restricción de dependencia: Xamarin.AndroidX.Fragment.Ktx 1.8.6.1 requiere Xamarin.AndroidX.Fragment (>= 1.8.6.1 && < 1.8.7), pero la versión Xamarin.AndroidX.Fragment 1.8.8.1 ya se resolvió.
1>E:\repos\tup\aplicada\2025\utn\Ejemplos_Maui_Devices\Ejemplo_Devices\Ejemplo_LectorQR\Ejemplo_LectorQR.csproj : warning NU1608: Se detectó una versión del paquete fuera de la restricción de dependencia: Xamarin.AndroidX.Lifecycle.Runtime.Ktx.Android 2.8.7.3 requiere Xamarin.AndroidX.Lifecycle.Runtime (>= 2.8.7.3 && < 2.8.8), pero la versión Xamarin.AndroidX.Lifecycle.Runtime 2.9.2.1 ya se resolvió.
1>MSBUILD : java.exe error JAVA0000: Error in obj\Debug\net9.0-android\lp\143\jl\classes.jar:androidx/savedstate/ViewKt.class:
1>MSBUILD : java.exe error JAVA0000: Type androidx.savedstate.ViewKt is defined multiple times: obj\Debug\net9.0-android\lp\143\jl\classes.jar:androidx/savedstate/ViewKt.class, obj\Debug\net9.0-android\lp\178\jl\classes.jar:androidx/savedstate/ViewKt.class
1>MSBUILD : java.exe error JAVA0000: Compilation failed
1>MSBUILD : java.exe error JAVA0000: java.lang.RuntimeException: com.android.tools.r8.CompilationFailedException: Compilation failed to complete, origin: obj\Debug\net9.0-android\lp\143\jl\classes.jar
1>MSBUILD : java.exe error JAVA0000: androidx/savedstate/ViewKt.class
1>MSBUILD : java.exe error JAVA0000: 	at com.android.tools.r8.internal.yu.a(R8_8.7.18_f8bee6d6fb926b7ebb3b15bf98f726f9d57471456ea20fce6d17d9a020197688:131)
1>MSBUILD : java.exe error JAVA0000: 	at com.android.tools.r8.D8.main(R8_8.7.18_f8bee6d6fb926b7ebb3b15bf98f726f9d57471456ea20fce6d17d9a020197688:5)
1>MSBUILD : java.exe error JAVA0000: Caused by: com.android.tools.r8.CompilationFailedException: Compilation failed to complete, origin: obj\Debug\net9.0-android\lp\143\jl\classes.jar:androidx/savedstate/ViewKt.class
1>MSBUILD : java.exe error JAVA0000: 	at Version.fakeStackEntry(Version_8.7.18.java:0)
1>MSBUILD : java.exe error JAVA0000: 	at com.android.tools.r8.T.a(R8_8.7.18_f8bee6d6fb926b7ebb3b15bf98f726f9d57471456ea20fce6d17d9a020197688:5)
1>MSBUILD : java.exe error JAVA0000: 	at com.android.tools.r8.internal.yu.a(R8_8.7.18_f8bee6d6fb926b7ebb3b15bf98f726f9d57471456ea20fce6d17d9a020197688:82)
1>MSBUILD : java.exe error JAVA0000: 	at com.android.tools.r8.internal.yu.a(R8_8.7.18_f8bee6d6fb926b7ebb3b15bf98f726f9d57471456ea20fce6d17d9a020197688:32)
1>MSBUILD : java.exe error JAVA0000: 	at com.android.tools.r8.internal.yu.a(R8_8.7.18_f8bee6d6fb926b7ebb3b15bf98f726f9d57471456ea20fce6d17d9a020197688:31)
1>MSBUILD : java.exe error JAVA0000: 	at com.android.tools.r8.internal.yu.b(R8_8.7.18_f8bee6d6fb926b7ebb3b15bf98f726f9d57471456ea20fce6d17d9a020197688:2)
1>MSBUILD : java.exe error JAVA0000: 	at com.android.tools.r8.D8.a(R8_8.7.18_f8bee6d6fb926b7ebb3b15bf98f726f9d57471456ea20fce6d17d9a020197688:42)
1>MSBUILD : java.exe error JAVA0000: 	at com.android.tools.r8.D8.b(R8_8.7.18_f8bee6d6fb926b7ebb3b15bf98f726f9d57471456ea20fce6d17d9a020197688:13)
1>MSBUILD : java.exe error JAVA0000: 	at com.android.tools.r8.D8.a(R8_8.7.18_f8bee6d6fb926b7ebb3b15bf98f726f9d57471456ea20fce6d17d9a020197688:40)
1>MSBUILD : java.exe error JAVA0000: 	at com.android.tools.r8.internal.yu.a(R8_8.7.18_f8bee6d6fb926b7ebb3b15bf98f726f9d57471456ea20fce6d17d9a020197688:118)
1>MSBUILD : java.exe error JAVA0000: 	... 1 more
1>MSBUILD : java.exe error JAVA0000: Caused by: com.android.tools.r8.internal.g: Type androidx.savedstate.ViewKt is defined multiple times: obj\Debug\net9.0-android\lp\143\jl\classes.jar:androidx/savedstate/ViewKt.class, obj\Debug\net9.0-android\lp\178\jl\classes.jar:androidx/savedstate/ViewKt.class
1>MSBUILD : java.exe error JAVA0000: 	at com.android.tools.r8.internal.bd0.a(R8_8.7.18_f8bee6d6fb926b7ebb3b15bf98f726f9d57471456ea20fce6d17d9a020197688:21)
1>MSBUILD : java.exe error JAVA0000: 	at com.android.tools.r8.internal.Z50.a(R8_8.7.18_f8bee6d6fb926b7ebb3b15bf98f726f9d57471456ea20fce6d17d9a020197688:54)
1>MSBUILD : java.exe error JAVA0000: 	at com.android.tools.r8.internal.Z50.a(R8_8.7.18_f8bee6d6fb926b7ebb3b15bf98f726f9d57471456ea20fce6d17d9a020197688:10)
1>MSBUILD : java.exe error JAVA0000: 	at java.base/java.util.concurrent.ConcurrentHashMap.merge(ConcurrentHashMap.java:2056)
1>MSBUILD : java.exe error JAVA0000: 	at com.android.tools.r8.internal.Z50.a(R8_8.7.18_f8bee6d6fb926b7ebb3b15bf98f726f9d57471456ea20fce6d17d9a020197688:6)
1>MSBUILD : java.exe error JAVA0000: 	at com.android.tools.r8.graph.s4$a.d(R8_8.7.18_f8bee6d6fb926b7ebb3b15bf98f726f9d57471456ea20fce6d17d9a020197688:6)
1>MSBUILD : java.exe error JAVA0000: 	at com.android.tools.r8.dex.c.a(R8_8.7.18_f8bee6d6fb926b7ebb3b15bf98f726f9d57471456ea20fce6d17d9a020197688:95)
1>MSBUILD : java.exe error JAVA0000: 	at com.android.tools.r8.dex.c.a(R8_8.7.18_f8bee6d6fb926b7ebb3b15bf98f726f9d57471456ea20fce6d17d9a020197688:44)
1>MSBUILD : java.exe error JAVA0000: 	at com.android.tools.r8.dex.c.a(R8_8.7.18_f8bee6d6fb926b7ebb3b15bf98f726f9d57471456ea20fce6d17d9a020197688:9)
1>MSBUILD : java.exe error JAVA0000: 	at com.android.tools.r8.D8.a(R8_8.7.18_f8bee6d6fb926b7ebb3b15bf98f726f9d57471456ea20fce6d17d9a020197688:45)
1>MSBUILD : java.exe error JAVA0000: 	at com.android.tools.r8.D8.d(R8_8.7.18_f8bee6d6fb926b7ebb3b15bf98f726f9d57471456ea20fce6d17d9a020197688:17)
1>MSBUILD : java.exe error JAVA0000: 	at com.android.tools.r8.D8.c(R8_8.7.18_f8bee6d6fb926b7ebb3b15bf98f726f9d57471456ea20fce6d17d9a020197688:71)
1>MSBUILD : java.exe error JAVA0000: 	at com.android.tools.r8.internal.yu.a(R8_8.7.18_f8bee6d6fb926b7ebb3b15bf98f726f9d57471456ea20fce6d17d9a020197688:28)
1>MSBUILD : java.exe error JAVA0000: 	... 6 more
1>MSBUILD : java.exe error JAVA0000: El directorio "obj\Debug\net9.0-android\lp\143" es de "androidx.savedstate.savedstate-android.aar".
1>MSBUILD : java.exe error JAVA0000: El directorio "obj\Debug\net9.0-android\lp\178" es de "androidx.savedstate.savedstate-ktx.aar".
========== Compilación: 0 correcto, 1 erróneo, 0 actualizado, 0 omitido ==========
========== Compilar completado a las 12:03 y tardó 02,817 segundos ==========
========== Implementación: 0 correcta, 0 con errores, 0 omitido ===========
========== Implementar completado a las 12:03 y tardó 02,817 segundos ==========