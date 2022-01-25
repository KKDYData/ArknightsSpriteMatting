# ArknightsSpriteMatting

Auto process matting of sprite assets from Arknights such as avatars and portraits.

自动抠图处理明日方舟中诸如头像和立绘的 Sprite 素材。

## How to Use 如何使用

1. Export all `Texture2D` files with [AssetStudio](https://github.com/Perfare/AssetStudio).

   使用 [AssetStudio](https://github.com/Perfare/AssetStudio) 导出所有的 `Texture2D` 文件。

   ![Export all Texture2D files with AssetStudio](https://user-images.githubusercontent.com/20498895/150927758-c75da122-c47c-4f19-bba3-0e78eaceef26.png)

2. Rename the exported `Texture2D` files to the corresponding `PathID`.

   将导出的 `Texture2D` 文件重命名为其对应的 `PathID`。

   ![Rename the exported Texture2D files to the corresponding PathID](https://user-images.githubusercontent.com/20498895/150927775-bad146a0-b8c3-4baf-a1c0-aa8e8aeb51c5.png)

   > ***Hint: In [AssetStudio](https://github.com/Perfare/AssetStudio), you can copy the `PathID` of a file by selecting the file and right-click on its `PathID`.***
   >
   > ***提示：在 [AssetStudio](https://github.com/Perfare/AssetStudio) 中，选中一个文件并在其 `PathID` 上右键即可复制其 `PathID`。***
   >
   > ![Select the file and right click on its PathID can copy the PathID](https://user-images.githubusercontent.com/20498895/150927780-5cffc4c1-6a50-4b94-a600-4faa3809f75d.png)

3. Select the `Sprite` files you want in [AssetStudio](https://github.com/Perfare/AssetStudio) and export to `Dump` files with the Menu Item `Export` -> `Dump` -> `Selected assets`.

   在 [AssetStudio](https://github.com/Perfare/AssetStudio) 中选中你需要的 `Sprite` 文件并在菜单栏中选择 `Export` -> `Dump` -> `Selected assets` 来导出 `转储` 文件。

   ![Export selected Sprite files to Dump](https://user-images.githubusercontent.com/20498895/150929135-8aa2f6fe-83bd-4211-a60f-262076ebc003.png)

4. Run the `executable` file downloaded from [Releases](https://github.com/KKDYData/ArknightsSpriteMatting/releases). After selecting `Texture2D Directory`, `Sprite Dump Directory`, `Output Directory` and several configuration items, click the `Confirm` button and wait for the processing to complete.

   运行从 [Releases](https://github.com/KKDYData/ArknightsSpriteMatting/releases) 中下载的`可执行`文件。选择 `Texture2D 目录`、`Sprite 转储目录`、`导出目录`以及几个配置项后，点击`确认`按钮等待处理完成。

   ![Selecting Texture2D Directory, Sprite Dump Directory and Output Directory](https://user-images.githubusercontent.com/20498895/150927784-e9dc6288-8705-459b-9a29-da9d9e99151e.png)
   ![Processing](https://user-images.githubusercontent.com/20498895/150927787-021965e5-743f-4620-a50e-bf394a582914.png)
   ![Processing complete](https://user-images.githubusercontent.com/20498895/150927792-d5111ffa-6e29-4277-af33-bd2b4aef8900.png)

