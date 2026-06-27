
## 202606262101_ejemplos
simulador rosseta para soporte de x64-> necesaria para la libreria de qr. esto obliga a incluir
```
	<ItemGroup Condition="'$(RuntimeIdentifier)' == 'iossimulator-x64'">
		<PackageReference Include="AdamE.Google.iOS.GoogleUtilities" Version="8.1.0.3" />		
	</ItemGroup>
```
por que la verison 9.0.1 no tiene esa libreria para x64

parametrización script simulacion

## 202606261239_ejemplos
primera versión de yml estandarizada
incopora mejoras en las rutas relativas. 