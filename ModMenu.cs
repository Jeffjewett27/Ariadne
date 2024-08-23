using System;
using Modding;
using Modding.Menu;
using Modding.Menu.Config;
using UnityEngine;
using Satchel.BetterMenus;
using System.IO;

namespace Ariadne
{
    public static class ModMenu
    {
        public static MenuScreen CreateMenuScreen(MenuScreen modListMenu)
        {
            int intervalMs = 1000;

            var menu = new Menu(
                "Ariadne Menu",
                new Element[]
                {
                    new HorizontalOption(
                         name: "Logging Active",
                        description: "Should logging be enabled?",
                        values: new [] { "Yes", "No" },
                        applySetting: index =>
                        {
                            Ariadne.settings.LoggingActive = index == 0; //"yes" is the 0th index in the values array
                        },
                        loadSetting: () => Ariadne.settings.LoggingActive ? 0 : 1),
                    new HorizontalOption(
                         name: "Show Hitboxes",
                        description: "Should show hitboxes?",
                        values: new [] { "Yes", "No" },
                        applySetting: index =>
                        {
                            Ariadne.settings.ShowHitBoxes = index == 0 ? ShowHitbox.VerboseLogs : ShowHitbox.None; //"yes" is the 0th index in the values array
                        },
                        loadSetting: () => Ariadne.settings.ShowHitBoxes == ShowHitbox.None ? 1 : 0),
                    new InputField(
                        name: "Log Folder",
                        _storeValue: val => Ariadne.settings.LogFolder = val,
                        _loadValue: () => { 
                            var val = Ariadne.settings.LogFolder;
                            return val.Length > 0 ? val : GlobalSettings.DefaultLogFolder;
                        },
                        _placeholder: "Path to log folder",
                        _characterLimit: 120,
                        _config: new Satchel.BetterMenus.Config.InputFieldConfig()
                        {
                            fontSize = 18,
                            inputBoxWidth = 800f,
                            saveType = Satchel.BetterMenus.Config.InputFieldSaveType.EditEnd
                        }
                    ),
                    new InputField(
                        name: "Logging Interval (MS)",
                        _storeValue: val => Ariadne.settings.LoggingIntervalMS = int.TryParse(val, out intervalMs) ? intervalMs : 1000,
                        _loadValue: () => Ariadne.settings.LoggingIntervalMS.ToString(),
                        _placeholder: "Path to log folder",
                        _characterLimit: 120,
                        _config: new Satchel.BetterMenus.Config.InputFieldConfig()
                        {
                            fontSize = 18,
                            inputBoxWidth = 800f,
                            contentType = UnityEngine.UI.InputField.ContentType.IntegerNumber,
                            saveType = Satchel.BetterMenus.Config.InputFieldSaveType.EditEnd
                        }
                    ),
                    //new CustomSlider(
                    //    name: "Logging Interval (MS)",
                    //    storeValue: val => Ariadne.settings.LoggingIntervalMS = (int)val,
                    //    loadValue: () => Ariadne.settings.LoggingIntervalMS,
                    //    minValue: 100,
                    //    maxValue: 10000,
                    //    wholeNumbers: true
                    //) 
                }
            );
            return menu.GetMenuScreen(modListMenu);
            //Action<MenuSelectable> CancelAction = selectable => UIManager.instance.UIGoToDynamicMenu(modListMenu);
            //return new MenuBuilder(UIManager.instance.UICanvas.gameObject, "Ariadne Menu")
            //    .CreateTitle("Ariadne Menu", MenuTitleStyle.vanillaStyle)
            //    .CreateContentPane(RectTransformData.FromSizeAndPos(
            //        new RelVector2(new Vector2(1920f, 903f)),
            //        new AnchoredPosition(
            //            new Vector2(0.5f, 0.5f),
            //            new Vector2(0.5f, 0.5f),
            //            new Vector2(0f, -60f)
            //        )
            //    ))
            //    .CreateControlPane(RectTransformData.FromSizeAndPos(
            //        new RelVector2(new Vector2(1920f, 259f)),
            //        new AnchoredPosition(
            //            new Vector2(0.5f, 0.5f),
            //            new Vector2(0.5f, 0.5f),
            //            new Vector2(0f, -502f)
            //        )
            //    ))
            //    .SetDefaultNavGraph(new GridNavGraph(1))
            //    .AddContent(
            //        RegularGridLayout.CreateVerticalLayout(105f),
            //        c =>
            //        {
            //            c.AddMenuButton(
            //                "Test Option",
            //                new MenuButtonConfig
            //                {
            //                    CancelAction = CancelAction,
            //                    SubmitAction = _ => Ariadne.MLog("test"),
            //                    Description = new DescriptionInfo
            //                    {
            //                        Text = "Just a button to test stuff"
            //                    },
            //                    Label = "Test option",
            //                    Proceed = false
            //                }
            //            )
            //            .AddTextPanel(
            //                "Text field",
            //                new RelVector2(new Vector2(600f, 50f)),
            //                new TextPanelConfig() { Text = "test text field" }
            //            );
            //        })
            //    .AddControls(
            //        new SingleContentLayout(new AnchoredPosition(
            //            new Vector2(0.5f, 0.5f),
            //            new Vector2(0.5f, 0.5f),
            //            new Vector2(0f, -64f)
            //        )), c => c.AddMenuButton(
            //            "BackButton",
            //            new MenuButtonConfig
            //            {
            //                Label = "Back",
            //                CancelAction = _ => UIManager.instance.UIGoToDynamicMenu(modListMenu),
            //                SubmitAction = _ => UIManager.instance.UIGoToDynamicMenu(modListMenu),
            //                Style = MenuButtonStyle.VanillaStyle,
            //                Proceed = true
            //            })); ;
        }
    }
}