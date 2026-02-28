<? xml version = "1.0" encoding = "utf-8" ?>
< manifest xmlns:android = "http://schemas.android.com/apk/res/android" >


    < !--Permisos para Bluetooth -->
    <uses-permission android:name = "android.permission.BLUETOOTH" />
    < uses - permission android: name = "android.permission.BLUETOOTH_ADMIN" />


    < !--Para Android 12+ (API 31+) -->
    <uses-permission android:name = "android.permission.BLUETOOTH_SCAN"
                     android: usesPermissionFlags = "neverForLocation" />
    < uses - permission android: name = "android.permission.BLUETOOTH_CONNECT" />


    < !--Permisos de ubicación(requeridos para escaneo Bluetooth en versiones antiguas) -->
    < uses - permission android: name = "android.permission.ACCESS_FINE_LOCATION" />
    < uses - permission android: name = "android.permission.ACCESS_COARSE_LOCATION" />


    < !--Declarar características de Bluetooth -->
    <uses-feature android:name = "android.hardware.bluetooth" android: required = "true" />
    < uses - feature android: name = "android.hardware.bluetooth_le" android: required = "false" />


    < application
        android: allowBackup = "true"
        android: icon = "@mipmap/appicon"
        android: roundIcon = "@mipmap/appicon_round"
        android: supportsRtl = "true" >


        < !--Query para habilitar Bluetooth -->
        <queries>
            <intent>
                <action android:name = "android.bluetooth.adapter.action.REQUEST_ENABLE" />
            </ intent >
        </ queries >


    </ application >


</ manifest >