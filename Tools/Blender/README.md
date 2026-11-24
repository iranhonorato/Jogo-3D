# Tools/Blender

Scripts Python (`bpy`) que geram os modelos 3D do jogo. Cada modelo é criado por um script salvo aqui, para poder ser gerado de novo quando algo mudar (GDD, seção 15.4).

## Scripts

| Script | Gera | Observações |
| --- | --- | --- |
| `char_raccoon.py` | `Char_Raccoon.fbx` | Guaxinim em pé, 0,6 m, máscara, cauda anelada e bolsa transversal |
| `char_guard.py` | `Char_Guard.fbx` | Segurança, 1,77 m, uniforme azul-marinho, quepe, lanterna (`Socket_Flashlight`) |
| `char_seu_ze.py` | `Char_SeuZe.fbx` | Seu Zé (dono e faxineiro do Mercadinho), 1,56 m, avental, vassoura, lanterna (`Socket_Flashlight`) |
| `hazard_security_camera.py` | `Hazards/Hazard_SecurityCamera.fbx` | Câmera de teto, pivô no ponto de fixação (z = 0, pende para baixo). A cabeça é o filho `Head`, com pivô no eixo da varredura (girar em Y na Unity). `Socket_Lens` na ponta da lente |
| `env_kit.py` | `Environment/Shelf_Aisle_2m.fbx`, `Shelf_Aisle_4m.fbx`, `Wall_2m.fbx`, `Floor_2m.fbx`, `Den_Manhole.fbx` | Kit modular com um atlas só (`M_Env_Kit`, `Env_Kit_BaseColor.png`). Gôndola dupla de 1,8 × 0,9 m, parede de 2 × 2,5 × 0,2 m, piso de 2 × 2 m com o topo em z = 0 e toca (bueiro aberto) |
| `items_kit.py` | `Items/Item_<Nome>.fbx` (os 19 itens da tabela §7.8) | Atlas `M_Items_Kit` (`Items_Kit_BaseColor.png`). Um pouco maiores que o real, para ler de longe. O `ItemFactory` usa o modelo quando o FBX existe e cai no cubo greybox quando não |
| `guaxinim_common.py` | — | Funções compartilhadas: geometria, pintura, bake, exportação e prévia. `run_character` para personagens e `run_model` para objetos com várias peças e textura compartilhada |

## Como rodar

Use o `blender.exe` do Blender 5.3, não o `blender-launcher.exe` do atalho:

```powershell
& "C:\Program Files\Blender Foundation\Blender 5.3\blender.exe" -b --factory-startup -P Tools/Blender/char_raccoon.py -- --preview <pasta>
```

`-b` roda sem interface, `--factory-startup` ignora as preferências locais e `-P` executa o script. O `--preview <pasta>` é opcional e renderiza uma imagem com três vistas (isométrica, 3/4 de frente e perfil) para conferir o resultado. Cada personagem leva menos de 30 s.

## Como os personagens são feitos

1. O script monta o personagem em alta resolução: corpo e membros com o modificador Skin, cabeça como esfera esculpida e acessórios encaixados por raycast na superfície do corpo.
2. Cada parte é pintada por regras (máscara do guaxinim, anéis da cauda, uniforme, xadrez da camisa) como cor por vértice.
3. Uma cópia de cada parte é simplificada até o orçamento de triângulos, sempre abaixo de 3 mil por personagem.
4. A cor da versão alta é assada (bake no Cycles) numa textura 1024×1024 da versão low poly, com variação de pincel e oclusão ambiente, para o visual de textura pintada.
5. O FBX vai para `Assets/_Project/Art/Models/Characters/` e a textura para `Assets/_Project/Art/Textures/Characters/<Nome>_BaseColor.png`.

Para mudar cores ou proporções, edite as constantes no topo do script do personagem (cores em hex sRGB) e rode de novo.

## Regras de exportação

- **Escala real em metros, pivô na base** (z = 0).
- **Frente do modelo em −Y no Blender**, que vira **+Z (forward)** na Unity. O +X do Blender vira −X na Unity, sem espelhar. Isso foi verificado importando um FBX de teste na Unity 6.3: a opção de eixo da exportação não muda o resultado, porque a Unity lê o sistema de eixos do arquivo.
- FBX exportado para `Assets/_Project/Art/Models/<pasta>/`, com o mesmo nome do prefab (`Item_Watermelon.fbx`, `Shelf_Aisle_2m.fbx`).
- **Material e textura:** o FBX traz o material `M_<Nome>`. O `BlenderModelPostprocessor` (em `Assets/_Project/Scripts/Editor/`) liga esse material à textura `Art/Textures/<pasta>/<Nome>_BaseColor.png` na importação. Siga essa convenção nos próximos modelos.
- **Pontos de encaixe:** empties chamados `Socket_<Uso>` viram GameObjects vazios na Unity (por exemplo, `Socket_Flashlight`, onde fica a luz spot da lanterna).
- Ícones dos itens: render ortográfico 256×256 com fundo transparente em `Assets/_Project/Art/Icons/`.
- Limite de triângulos: guaxinim e segurança até 3,3 mil, itens até 500, peças de cenário até 1.500 (a gôndola de 4 m tem cerca de 2.900, com os potes).
- Texturas agrupadas em atlas por conjunto (personagens, itens, cenário) para manter poucos draw calls.

O estilo visual segue a seção 15.1 do GDD e a imagem de referência em `docs/`.

## Ainda não feito

- Rig e animações (andar, correr, carregar, pular, pego). Os modelos são estáticos.
- Variação do guaxinim no modo cogumelo e uma variação de uniforme por mercado.
- Atlas único para os personagens (hoje cada um tem a sua textura).
