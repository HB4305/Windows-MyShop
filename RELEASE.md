# Release Output

Build the desktop release with:

```bash
dotnet build MyShop/MyShop.csproj -f net10.0-desktop -c Release
```

The build copies the executable output to the top-level `Release` folder.
The file to run is immediately visible here:

```text
Release/MyShop.exe
```

Keep every generated file in the `Release` folder together. If an installer is created later, place it in the same `Release` folder.
