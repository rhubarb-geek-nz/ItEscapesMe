# rhubarb-geek-nz.ItEscapesMe
Escapology for PowerShell

```
ConvertTo-EscapeString [-InputString] <string> [<CommonParameters>]
```

Example for PowerShell 7

```
PS> "Hello World`u{D}`u{A}" | ConvertTo-EscapeString
Hello World`r`n
PS> [Text.Rune]0x1F600 | ConvertTo-EscapeString
`u{1F600}
PS> "`u{1F600}`u{1F601}"
😀😁
PS> "`u{1F600}`u{1F601}" | ConvertTo-EscapeString
`u{1F600}`u{1F601}
PS> "😀😁" | ConvertTo-EscapeString
`u{1F600}`u{1F601}
PS> [Text.Rune[]](0x1F600,0x1F601) | ConvertTo-EscapeString
`u{1F600}
`u{1F601}
PS> [Text.Rune[]](0x1F600,0x1F601) | ConvertTo-EscapeString | Out-String -NoNewline
`u{1F600}`u{1F601}
```

Example for WindowsPowerShell

```
PS> [char]127 | ConvertTo-EscapeString
$([char]127)
```

Build module from command prompt within the [ItEscapesMe](ItEscapesMe) directory

```
PS> dotnet publish --configuration Release
Restore complete (0.8s)
  ItEscapesMe netstandard2.0 succeeded (2.9s) → bin\Release\netstandard2.0\publish\

Build succeeded in 4.4s
```
