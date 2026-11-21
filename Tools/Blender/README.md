# Tools/Blender

Scripts Python (`bpy`) que geram os modelos 3D do jogo. Cada modelo é criado por um script salvo aqui, para poder ser gerado de novo quando algo mudar (GDD, seção 15.4).

## Como rodar

Use o `blender.exe` do Blender 5.3, não o `blender-launcher.exe` do atalho:

```powershell
& "C:\Program Files\Blender Foundation\Blender 5.3\blender.exe" -b -P Tools/Blender/<script>.py
```

`-b` roda sem interface e `-P` executa o script.

## Regras de exportação

- Escala real em metros, pivô na base do objeto.
- FBX exportado para `Assets/_Project/Art/Models/`, com o mesmo nome do prefab (`Item_Watermelon.fbx`, `Shelf_Aisle_2m.fbx`).
- Ícones dos itens: render ortográfico 256×256 com fundo transparente em `Assets/_Project/Art/Icons/`.
- Limite de triângulos: guaxinim e segurança até 3 mil, itens até 500, peças de cenário até 1.500.
- Texturas agrupadas em atlas por conjunto (personagens, itens, cenário) para manter poucos draw calls.

O estilo visual segue a seção 15.1 do GDD e a imagem de referência em `docs/`.
