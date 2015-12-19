using System;
using System.Drawing;
using LeagueSharp;
using LeagueSharp.Common;

namespace SharpShooter
{
    /// <summary>
    ///     메인메뉴를 간편하게 조작할 수 있는 클래스입니다.
    /// </summary>
    internal class MenuProvider
    {
        /// <summary>
        ///     메인메뉴의 LeagueSharp.Common.Menu형식 객체입니다. 이 객체에 직접 접근하면 LeagueSharp.Common.Menu클래스의 메소드를 이용해 메인메뉴를 수정 할 수 있습니다.
        /// </summary>
        internal static Menu MenuInstance;

        internal static Orbwalking.Orbwalker Orbwalker;

        internal static Menu ChampionMenuInstance
        {
            get { return MenuInstance.SubMenu("Champion"); }
        }

        /// <summary>
        ///     메인메뉴를 만들고 root메뉴에 추가합니다.
        /// </summary>
        internal static void Initialize()
        {
            if (PluginLoader.CanLoadPlugin(ObjectManager.Player.ChampionName))
            {
                MenuInstance = new Menu("#Shooter Reworked: OK", "SharpShooterCommon", true);

                AddSubMenu("Champion", ObjectManager.Player.ChampionName);

                Champion.AddOrbwalker();
                Champion.AddTargetSelector();
            }
            else
            {
                MenuInstance = new Menu("#Shooter Reworked: X", "SharpShooterCommon", true);

                AddItem("txt1", "Sorry. " + ObjectManager.Player.ChampionName + " is not supported yet.", null);
                Game.PrintChat(
                    "<font color = \"#00D8FF\"><b>SharpShooter Reworked:</b></font> <font color = \"#FF007F\">" +
                    ObjectManager.Player.ChampionName + "</font> is not supported yet.");
            }

            MenuInstance.AddToMainMenu();

            Console.WriteLine("SharpShooter: MenuProvider Initialized.");
        }

        /// <summary>
        ///     메인메뉴에 서브메뉴를 추가합니다.
        /// </summary>
        /// <param name="displayName">추가할 서브메뉴의 표기이름을 기입하십시오.</param>
        internal static void AddSubMenu(string displayName)
        {
            MenuInstance.AddSubMenu(new Menu(displayName, displayName));
        }

        /// <summary>
        ///     메인메뉴에 서브메뉴를 추가합니다.
        /// </summary>
        /// <param name="name">추가할 서브메뉴의 이름을 기입합니다.</param>
        /// <param name="displayName">추가할 서브메뉴의 표기이름을 기입하십시오.</param>
        internal static void AddSubMenu(string name, string displayName)
        {
            MenuInstance.AddSubMenu(new Menu(displayName, name));
        }

        /// <summary>
        ///     메인메뉴에 항목을 추가합니다.
        /// </summary>
        /// <param name="displayName">추가할 항목의 표기이름을 기입하십시오.</param>
        /// <param name="value">항목의 값을 기입하십시오.</param>
        /// <param name="champUniq">이 값을 true로 지정하면 플레이어의 항목의 이름에 챔피언이름을 포함한 항목이 생성됩니다.</param>
        internal static void AddItem(string displayName, object value = null, bool champUniq = false)
        {
            if (value == null)
            {
                MenuInstance.AddItem(new MenuItem(displayName, displayName, champUniq));
                return;
            }

            MenuInstance.AddItem(new MenuItem(displayName, displayName, champUniq)).SetValue(value);
        }

        /// <summary>
        ///     메인메뉴에 항목을 추가합니다.
        /// </summary>
        /// <param name="name">추가할 항목의 이름을 기입하십시오.</param>
        /// <param name="displayName">추가할 항목의 표기이름을 기입하십시오.</param>
        /// <param name="value">항목의 값을 기입하십시오.</param>
        /// <param name="champUniq">이 값을 true로 지정하면 플레이어의 항목의 이름에 챔피언이름을 포함한 항목이 생성됩니다.</param>
        internal static void AddItem(string name, string displayName, object value = null, bool champUniq = false)
        {
            if (value == null)
            {
                MenuInstance.AddItem(new MenuItem(name, displayName, champUniq));
                return;
            }

            MenuInstance.AddItem(new MenuItem(name, displayName, champUniq)).SetValue(value);
        }

        /// <summary>
        ///     이 클래스를 이용해서 이 Activator 메뉴에 접근할 수 있습니다.
        /// </summary>
        internal class Activator
        {
            /// <summary>
            ///     SupportedChampions 메뉴에 항목을 추가합니다.
            /// </summary>
            /// <param name="displayName">추가할 항목의 표기이름을 기입하십시오.</param>
            /// <param name="value">항목의 값을 기입하십시오.</param>
            /// <param name="champUniq">이 값을 true로 지정하면 플레이어의 항목의 이름에 챔피언이름을 포함한 항목이 생성됩니다.</param>
            internal static void AddItem(string displayName, object value = null, bool champUniq = true)
            {
                if (value == null)
                {
                    MenuInstance.SubMenu("Activator").AddItem(new MenuItem(displayName, displayName, champUniq));
                    return;
                }

                MenuInstance.SubMenu("Activator")
                    .AddItem(new MenuItem(displayName, displayName, champUniq))
                    .SetValue(value);
            }
        }

        /// <summary>
        ///     이 클래스를 이용해서 이 SupportedChampions 메뉴에 접근할 수 있습니다.
        /// </summary>
        internal class SupportedChampions
        {
            /// <summary>
            ///     SupportedChampions 메뉴에 항목을 추가합니다.
            /// </summary>
            /// <param name="displayName">추가할 항목의 표기이름을 기입하십시오.</param>
            /// <param name="value">항목의 값을 기입하십시오.</param>
            /// <param name="champUniq">이 값을 true로 지정하면 플레이어의 항목의 이름에 챔피언이름을 포함한 항목이 생성됩니다.</param>
            internal static void AddItem(string displayName, object value = null, bool champUniq = true)
            {
                if (value == null)
                {
                    MenuInstance.SubMenu("Supported champions")
                        .AddItem(new MenuItem(displayName, displayName, champUniq));
                    return;
                }

                MenuInstance.SubMenu("Supported champions")
                    .AddItem(new MenuItem(displayName, displayName, champUniq))
                    .SetValue(value);
            }
        }

        /// <summary>
        ///     이 클래스를 이용해서 챔피언 메뉴에 접근할 수 있습니다.
        /// </summary>
        internal class Champion
        {
            /// <summary>
            ///     챔피언 메뉴에 항목을 추가합니다.
            /// </summary>
            /// <param name="displayName">항목의 표기이름을 기입하십시오. 이 함수는 항목의 Name도 DisplayName매개변수의 값으로 설정합니다.</param>
            /// <param name="value">항목의 초기값을 기입하십시오.</param>
            /// <param name="champUniq">이 값을 true로 지정하면 항목의 이름에 플레이어의 챔피언이름을 포함한 항목이 생성됩니다.</param>
            internal static void AddItem(string displayName, object value = null, bool champUniq = true)
            {
                if (value == null)
                {
                    MenuInstance.SubMenu("Champion").AddItem(new MenuItem(displayName, displayName, champUniq));
                    return;
                }

                MenuInstance.SubMenu("Champion")
                    .AddItem(new MenuItem(displayName, displayName, champUniq))
                    .SetValue(value);
            }

            /// <summary>
            ///     챔피언 메뉴에 오브워커를 추가합니다.
            /// </summary>
            internal static void AddOrbwalker()
            {
                Orbwalker =
                    new Orbwalking.Orbwalker(
                        MenuInstance.SubMenu("Champion").AddSubMenu(new Menu("Orbwalker", "Orbwalker")));
            }

            /// <summary>
            ///     챔피언 메뉴에 타겟셀렉터를 추가합니다.
            /// </summary>
            internal static void AddTargetSelector()
            {
                TargetSelector.AddToMenu(
                    MenuInstance.SubMenu("Champion").AddSubMenu(new Menu("Target Selector", "Target Selector")));
            }

            /// <summary>
            ///     이 클래스를 이용해서 Combo 메뉴에 접근할 수 있습니다.
            /// </summary>
            internal class Combo
            {
                /// <summary>
                ///     'Use Q' 항목의 값을 반환합니다.
                /// </summary>
                internal static bool UseQ
                {
                    get { return GetBoolValue("Use Q"); }
                }

                /// <summary>
                ///     'Use W' 항목의 값을 반환합니다.
                /// </summary>
                internal static bool UseW
                {
                    get { return GetBoolValue("Use W"); }
                }

                /// <summary>
                ///     'Use E' 항목의 값을 반환합니다.
                /// </summary>
                internal static bool UseE
                {
                    get { return GetBoolValue("Use E"); }
                }

                /// <summary>
                ///     'Use R' 항목의 값을 반환합니다.
                /// </summary>
                internal static bool UseR
                {
                    get { return GetBoolValue("Use R"); }
                }

                /// <summary>
                ///     'If Mana >' 항목의 값을 반환합니다.
                /// </summary>
                internal static int IfMana
                {
                    get { return GetSliderValue("If Mana >").Value; }
                }

                /// <summary>
                ///     Combo 메뉴에 항목을 추가합니다.
                /// </summary>
                /// <param name="displayName">항목의 표기이름을 기입하십시오.</param>
                /// <param name="value">항목의 초기값을 기입하십시오.</param>
                /// <param name="champUniq">이 값을 true로 지정하면 항목의 이름에 플레이어의 챔피언이름을 포함한 항목이 생성됩니다.</param>
                internal static void AddItem(string displayName, object value = null, bool champUniq = true)
                {
                    if (value == null)
                    {
                        MenuInstance.SubMenu("Champion")
                            .SubMenu("Combo")
                            .AddItem(new MenuItem("Combo." + displayName, displayName, champUniq));
                        return;
                    }

                    MenuInstance.SubMenu("Champion")
                        .SubMenu("Combo")
                        .AddItem(new MenuItem("Combo." + displayName, displayName, champUniq))
                        .SetValue(value);
                }

                /// <summary>
                ///     Combo 메뉴 안에 존재하는 항목의 Boolean값을 가져옵니다.
                /// </summary>
                /// <param name="displayName">값을 가져올 항목의 표기이름을 기입하십시오.</param>
                /// <param name="champUniq">이 값을 true로 지정하면 플레이어의 챔피언 이름을 포함한 이름을 가진 항목의 값을 가져옵니다.</param>
                /// <returns>항목의 bool값을 반환합니다.</returns>
                internal static bool GetBoolValue(string displayName, bool champUniq = true)
                {
                    return MenuInstance.Item("Combo." + displayName, champUniq).GetValue<bool>();
                }

                /// <summary>
                ///     Combo 메뉴 안에 존재하는 항목의 LeageuSharp.Common.KeyBind 구조체를 가져옵니다.
                /// </summary>
                /// <param name="displayName"></param>
                /// <param name="champUniq"></param>
                /// <returns>LeageuSharp.Common.KeyBind 구조체를 반환합니다.</returns>
                internal static KeyBind GetKeyBindValue(string displayName, bool champUniq = true)
                {
                    return MenuInstance.Item("Combo." + displayName, champUniq).GetValue<KeyBind>();
                }

                /// <summary>
                ///     Combo 메뉴 안에 존재하는 항목의 LeageuSharp.Common.Slider 구조체를 가져옵니다.
                /// </summary>
                /// <param name="displayName"></param>
                /// <param name="champUniq"></param>
                /// <returns>LeageuSharp.Common.Slider 구조체를 반환합니다.</returns>
                internal static Slider GetSliderValue(string displayName, bool champUniq = true)
                {
                    return MenuInstance.Item("Combo." + displayName, champUniq).GetValue<Slider>();
                }

                internal static StringList GetStringListValue(string displayName, bool champUniq = true)
                {
                    return MenuInstance.Item("Combo." + displayName, champUniq).GetValue<StringList>();
                }

                /// <summary>
                ///     Combo 메뉴에 'Use Q' 항목을 추가합니다..
                /// </summary>
                /// <param name="enabled">항목의 초기값을 기입하십시오.</param>
                internal static void AddUseQ(bool enabled = true)
                {
                    AddItem("Use Q", enabled);
                }

                /// <summary>
                ///     Combo 메뉴에 'Use W' 항목을 추가합니다..
                /// </summary>
                /// <param name="enabled">항목의 초기값을 기입하십시오.</param>
                internal static void AddUseW(bool enabled = true)
                {
                    AddItem("Use W", enabled);
                }

                /// <summary>
                ///     Combo 메뉴에 'Use E' 항목을 추가합니다..
                /// </summary>
                /// <param name="enabled">항목의 초기값을 기입하십시오.</param>
                internal static void AddUseE(bool enabled = true)
                {
                    AddItem("Use E", enabled);
                }

                /// <summary>
                ///     Combo 메뉴에 'Use R' 항목을 추가합니다..
                /// </summary>
                /// <param name="enabled">항목의 초기값을 기입하십시오.</param>
                internal static void AddUseR(bool enabled = true)
                {
                    AddItem("Use R", enabled);
                }

                /// <summary>
                ///     Combo 메뉴에 'If Mana >' 항목을 추가합니다.
                /// </summary>
                /// <param name="defaultValue">항목의 초기값을 기입하십시오.</param>
                internal static void AddIfMana(int defaultValue = 0)
                {
                    AddItem("If Mana >", new Slider(defaultValue));
                }
            }

            /// <summary>
            ///     이 클래스를 이용해서 Harass 메뉴에 접근할 수 있습니다.
            /// </summary>
            internal class Harass
            {
                /// <summary>
                ///     'Auto Harass' 항목의 값을 반환합니다.
                /// </summary>
                internal static bool AutoHarass
                {
                    get { return GetBoolValue("Auto Harass"); }
                }

                /// <summary>
                ///     'Use Q' 항목의 값을 반환합니다.
                /// </summary>
                internal static bool UseQ
                {
                    get { return GetBoolValue("Use Q"); }
                }

                /// <summary>
                ///     'Use W' 항목의 값을 반환합니다.
                /// </summary>
                internal static bool UseW
                {
                    get { return GetBoolValue("Use W"); }
                }

                /// <summary>
                ///     'Use E' 항목의 값을 반환합니다.
                /// </summary>
                internal static bool UseE
                {
                    get { return GetBoolValue("Use E"); }
                }

                /// <summary>
                ///     'Use R' 항목의 값을 반환합니다.
                /// </summary>
                internal static bool UseR
                {
                    get { return GetBoolValue("Use R"); }
                }

                /// <summary>
                ///     'If Mana >' 항목의 값을 반환합니다.
                /// </summary>
                internal static int IfMana
                {
                    get { return GetSliderValue("If Mana >").Value; }
                }

                /// <summary>
                ///     Harass 메뉴에 항목을 추가합니다.
                /// </summary>
                /// <param name="displayName">항목의 표기이름을 기입하십시오.</param>
                /// <param name="value">항목의 초기값을 기입하십시오.</param>
                /// <param name="champUniq">이 값을 true로 지정하면 항목의 이름에 플레이어의 챔피언이름을 포함한 항목이 생성됩니다.</param>
                internal static void AddItem(string displayName, object value = null, bool champUniq = true)
                {
                    if (value == null)
                    {
                        MenuInstance.SubMenu("Champion")
                            .SubMenu("Harass")
                            .AddItem(new MenuItem("Harass." + displayName, displayName, champUniq));
                        return;
                    }

                    MenuInstance.SubMenu("Champion")
                        .SubMenu("Harass")
                        .AddItem(new MenuItem("Harass." + displayName, displayName, champUniq))
                        .SetValue(value);
                }

                /// <summary>
                ///     Harass 메뉴에 존재하는 항목의 Boolean값을 가져옵니다.
                /// </summary>
                /// <param name="displayName">값을 가져올 항목의 표기이름을 기입하십시오.</param>
                /// <param name="champUniq">이 값을 true로 지정하면 플레이어의 챔피언 이름을 포함한 이름을 가진 항목의 값을 가져옵니다.</param>
                /// <returns>항목의 bool값을 반환합니다.</returns>
                internal static bool GetBoolValue(string displayName, bool champUniq = true)
                {
                    return MenuInstance.Item("Harass." + displayName, champUniq).GetValue<bool>();
                }

                /// <summary>
                ///     Harass 메뉴에 존재하는 항목의 LeageuSharp.Common.Slider 구조체를 가져옵니다.
                /// </summary>
                /// <param name="displayName"></param>
                /// <param name="champUniq"></param>
                /// <returns>LeageuSharp.Common.Slider 구조체를 반환합니다.</returns>
                internal static Slider GetSliderValue(string displayName, bool champUniq = true)
                {
                    return MenuInstance.Item("Harass." + displayName, champUniq).GetValue<Slider>();
                }

                /// <summary>
                ///     Harass 메뉴에 'Auto Harass' 항목을 추가합니다.
                /// </summary>
                /// <param name="enabled">항목의 초기값을 기입하십시오.</param>
                internal static void AddAutoHarass(bool enabled = true)
                {
                    AddItem("Auto Harass", enabled);
                }

                /// <summary>
                ///     Harass 메뉴에 'Use Q' 항목을 추가합니다..
                /// </summary>
                /// <param name="enabled">항목의 초기값을 기입하십시오.</param>
                internal static void AddUseQ(bool enabled = true)
                {
                    AddItem("Use Q", enabled);
                }

                /// <summary>
                ///     Harass 메뉴에 'Use W' 항목을 추가합니다..
                /// </summary>
                /// <param name="enabled">항목의 초기값을 기입하십시오.</param>
                internal static void AddUseW(bool enabled = true)
                {
                    AddItem("Use W", enabled);
                }

                /// <summary>
                ///     Harass 메뉴에 'Use E' 항목을 추가합니다..
                /// </summary>
                /// <param name="enabled">항목의 초기값을 기입하십시오.</param>
                internal static void AddUseE(bool enabled = true)
                {
                    AddItem("Use E", enabled);
                }

                /// <summary>
                ///     Harass 메뉴에 'Use R' 항목을 추가합니다..
                /// </summary>
                /// <param name="enabled">항목의 초기값을 기입하십시오.</param>
                internal static void AddUseR(bool enabled = true)
                {
                    AddItem("Use R", enabled);
                }

                /// <summary>
                ///     Harass 메뉴에 'If Mana >' 항목을 추가합니다.
                /// </summary>
                /// <param name="defaultValue">항목의 초기값을 기입하십시오.</param>
                internal static void AddIfMana(int defaultValue = 60)
                {
                    AddItem("If Mana >", new Slider(defaultValue));
                }
            }

            /// <summary>
            ///     이 클래스를 이용해서 Lasthit 메뉴에 접근할 수 있습니다.
            /// </summary>
            internal class Lasthit
            {
                /// <summary>
                ///     'Use Q' 항목의 값을 반환합니다.
                /// </summary>
                internal static bool UseQ
                {
                    get { return GetBoolValue("Use Q"); }
                }

                /// <summary>
                ///     'Use W' 항목의 값을 반환합니다.
                /// </summary>
                internal static bool UseW
                {
                    get { return GetBoolValue("Use W"); }
                }

                /// <summary>
                ///     'Use E' 항목의 값을 반환합니다.
                /// </summary>
                internal static bool UseE
                {
                    get { return GetBoolValue("Use E"); }
                }

                /// <summary>
                ///     'Use R' 항목의 값을 반환합니다.
                /// </summary>
                internal static bool UseR
                {
                    get { return GetBoolValue("Use R"); }
                }

                /// <summary>
                ///     'If Mana >' 항목의 값을 반환합니다.
                /// </summary>
                internal static int IfMana
                {
                    get { return GetSliderValue("If Mana >").Value; }
                }

                /// <summary>
                ///     Lasthit 메뉴에 항목을 추가합니다.
                /// </summary>
                /// <param name="displayName">항목의 표기이름을 기입하십시오.</param>
                /// <param name="value">항목의 초기값을 기입하십시오.</param>
                /// <param name="champUniq">이 값을 true로 지정하면 항목의 이름에 플레이어의 챔피언이름을 포함한 항목이 생성됩니다.</param>
                internal static void AddItem(string displayName, object value = null, bool champUniq = true)
                {
                    if (value == null)
                    {
                        MenuInstance.SubMenu("Champion")
                            .SubMenu("Lasthit")
                            .AddItem(new MenuItem("Lasthit." + displayName, displayName, champUniq));
                        return;
                    }

                    MenuInstance.SubMenu("Champion")
                        .SubMenu("Lasthit")
                        .AddItem(new MenuItem("Lasthit." + displayName, displayName, champUniq))
                        .SetValue(value);
                }

                /// <summary>
                ///     Lasthit 메뉴 안에 존재하는 항목의 Boolean값을 가져옵니다.
                /// </summary>
                /// <param name="displayName">값을 가져올 항목의 표기이름을 기입하십시오.</param>
                /// <param name="champUniq">이 값을 true로 지정하면 플레이어의 챔피언 이름을 포함한 이름을 가진 항목의 값을 가져옵니다.</param>
                internal static bool GetBoolValue(string displayName, bool champUniq = true)
                {
                    return MenuInstance.Item("Lasthit." + displayName, champUniq).GetValue<bool>();
                }

                /// <summary>
                ///     Lasthit 메뉴 안에 존재하는 항목의 LeageuSharp.Common.Slider 구조체를 가져옵니다.
                /// </summary>
                /// <param name="displayName"></param>
                /// <param name="champUniq"></param>
                /// <returns>LeageuSharp.Common.Slider 구조체를 반환합니다.</returns>
                internal static Slider GetSliderValue(string displayName, bool champUniq = true)
                {
                    return MenuInstance.Item("Lasthit." + displayName, champUniq).GetValue<Slider>();
                }

                /// <summary>
                ///     Lasthit 메뉴에 'Use Q' 항목을 추가합니다..
                /// </summary>
                /// <param name="enabled">항목의 초기값을 기입하십시오.</param>
                internal static void AddUseQ(bool enabled = true)
                {
                    AddItem("Use Q", enabled);
                }

                /// <summary>
                ///     Lasthit 메뉴에 'Use W' 항목을 추가합니다..
                /// </summary>
                /// <param name="enabled">항목의 초기값을 기입하십시오.</param>
                internal static void AddUseW(bool enabled = true)
                {
                    AddItem("Use W", enabled);
                }

                /// <summary>
                ///     Lasthit 메뉴에 'Use E' 항목을 추가합니다..
                /// </summary>
                /// <param name="enabled">항목의 초기값을 기입하십시오.</param>
                internal static void AddUseE(bool enabled = true)
                {
                    AddItem("Use E", enabled);
                }

                /// <summary>
                ///     Lasthit 메뉴에 'Use R' 항목을 추가합니다..
                /// </summary>
                /// <param name="enabled">항목의 초기값을 기입하십시오.</param>
                internal static void AddUseR(bool enabled = true)
                {
                    AddItem("Use R", enabled);
                }

                /// <summary>
                ///     Lasthit 메뉴에 'If Mana >' 항목을 추가합니다.
                /// </summary>
                /// <param name="defaultValue">항목의 초기값을 기입하십시오.</param>
                internal static void AddIfMana(int defaultValue = 60)
                {
                    AddItem("If Mana >", new Slider(defaultValue));
                }
            }

            /// <summary>
            ///     이 클래스를 이용해서 Laneclear 메뉴에 접근할 수 있습니다.
            /// </summary>
            internal class Laneclear
            {
                /// <summary>
                ///     'Use Q' 항목의 값을 반환합니다.
                /// </summary>
                internal static bool UseQ
                {
                    get { return GetBoolValue("Use Q"); }
                }

                /// <summary>
                ///     'Use W' 항목의 값을 반환합니다.
                /// </summary>
                internal static bool UseW
                {
                    get { return GetBoolValue("Use W"); }
                }

                /// <summary>
                ///     'Use E' 항목의 값을 반환합니다.
                /// </summary>
                internal static bool UseE
                {
                    get { return GetBoolValue("Use E"); }
                }

                /// <summary>
                ///     'Use R' 항목의 값을 반환합니다.
                /// </summary>
                internal static bool UseR
                {
                    get { return GetBoolValue("Use R"); }
                }

                /// <summary>
                ///     'If Mana >' 항목의 값을 반환합니다.
                /// </summary>
                internal static int IfMana
                {
                    get { return GetSliderValue("If Mana >").Value; }
                }

                /// <summary>
                ///     Laneclear 메뉴에 항목을 추가합니다.
                /// </summary>
                /// <param name="displayName">항목의 표기이름을 기입하십시오.</param>
                /// <param name="value">항목의 초기값을 기입하십시오.</param>
                /// <param name="champUniq">이 값을 true로 지정하면 항목의 이름에 플레이어의 챔피언이름을 포함한 항목이 생성됩니다.</param>
                internal static void AddItem(string displayName, object value = null, bool champUniq = true)
                {
                    if (value == null)
                    {
                        MenuInstance.SubMenu("Champion")
                            .SubMenu("Laneclear")
                            .AddItem(new MenuItem("Laneclear." + displayName, displayName, champUniq));
                        return;
                    }

                    MenuInstance.SubMenu("Champion")
                        .SubMenu("Laneclear")
                        .AddItem(new MenuItem("Laneclear." + displayName, displayName, champUniq))
                        .SetValue(value);
                }

                /// <summary>
                ///     Laneclear 메뉴 안에 존재하는 항목의 Boolean값을 가져옵니다.
                /// </summary>
                /// <param name="displayName">값을 가져올 항목의 표기이름을 기입하십시오.</param>
                /// <param name="champUniq">이 값을 true로 지정하면 플레이어의 챔피언 이름을 포함한 이름을 가진 항목의 값을 가져옵니다.</param>
                internal static bool GetBoolValue(string displayName, bool champUniq = true)
                {
                    return MenuInstance.Item("Laneclear." + displayName, champUniq).GetValue<bool>();
                }

                /// <summary>
                ///     Laneclear 메뉴 안에 존재하는 항목의 LeageuSharp.Common.Slider 구조체를 가져옵니다.
                /// </summary>
                /// <param name="displayName"></param>
                /// <param name="champUniq"></param>
                /// <returns>LeageuSharp.Common.Slider 구조체를 반환합니다.</returns>
                internal static Slider GetSliderValue(string displayName, bool champUniq = true)
                {
                    return MenuInstance.Item("Laneclear." + displayName, champUniq).GetValue<Slider>();
                }

                /// <summary>
                ///     Laneclear 메뉴에 'Use Q' 항목을 추가합니다..
                /// </summary>
                /// <param name="enabled">항목의 초기값을 기입하십시오.</param>
                internal static void AddUseQ(bool enabled = true)
                {
                    AddItem("Use Q", enabled);
                }

                /// <summary>
                ///     Laneclear 메뉴에 'Use W' 항목을 추가합니다..
                /// </summary>
                /// <param name="enabled">항목의 초기값을 기입하십시오.</param>
                internal static void AddUseW(bool enabled = true)
                {
                    AddItem("Use W", enabled);
                }

                /// <summary>
                ///     Laneclear 메뉴에 'Use E' 항목을 추가합니다..
                /// </summary>
                /// <param name="enabled">항목의 초기값을 기입하십시오.</param>
                internal static void AddUseE(bool enabled = true)
                {
                    AddItem("Use E", enabled);
                }

                /// <summary>
                ///     Laneclear 메뉴에 'Use R' 항목을 추가합니다..
                /// </summary>
                /// <param name="enabled">항목의 초기값을 기입하십시오.</param>
                internal static void AddUseR(bool enabled = true)
                {
                    AddItem("Use R", enabled);
                }

                /// <summary>
                ///     Laneclear 메뉴에 'If Mana >' 항목을 추가합니다.
                /// </summary>
                /// <param name="defaultValue">항목의 초기값을 기입하십시오.</param>
                internal static void AddIfMana(int defaultValue = 0)
                {
                    AddItem("If Mana >", new Slider(defaultValue));
                }
            }

            /// <summary>
            ///     이 클래스를 이용해서 Jungleclear 메뉴에 접근할 수 있습니다.
            /// </summary>
            internal class Jungleclear
            {
                /// <summary>
                ///     'Use Q' 항목의 값을 반환합니다.
                /// </summary>
                internal static bool UseQ
                {
                    get { return GetBoolValue("Use Q"); }
                }

                /// <summary>
                ///     'Use W' 항목의 값을 반환합니다.
                /// </summary>
                internal static bool UseW
                {
                    get { return GetBoolValue("Use W"); }
                }

                /// <summary>
                ///     'Use E' 항목의 값을 반환합니다.
                /// </summary>
                internal static bool UseE
                {
                    get { return GetBoolValue("Use E"); }
                }

                /// <summary>
                ///     'Use R' 항목의 값을 반환합니다.
                /// </summary>
                internal static bool UseR
                {
                    get { return GetBoolValue("Use R"); }
                }

                /// <summary>
                ///     'If Mana >' 항목의 값을 반환합니다.
                /// </summary>
                internal static int IfMana
                {
                    get { return GetSliderValue("If Mana >").Value; }
                }

                /// <summary>
                ///     Jungleclear 메뉴에 항목을 추가합니다.
                /// </summary>
                /// <param name="displayName">항목의 표기이름을 기입하십시오.</param>
                /// <param name="value">항목의 초기값을 기입하십시오.</param>
                /// <param name="champUniq">이 값을 true로 지정하면 항목의 이름에 플레이어의 챔피언이름을 포함한 항목이 생성됩니다.</param>
                internal static void AddItem(string displayName, object value = null, bool champUniq = true)
                {
                    if (value == null)
                    {
                        MenuInstance.SubMenu("Champion")
                            .SubMenu("Jungleclear")
                            .AddItem(new MenuItem("Jungleclear." + displayName, displayName, champUniq));
                        return;
                    }

                    MenuInstance.SubMenu("Champion")
                        .SubMenu("Jungleclear")
                        .AddItem(new MenuItem("Jungleclear." + displayName, displayName, champUniq))
                        .SetValue(value);
                }

                /// <summary>
                ///     Jungleclear 메뉴 안에 존재하는 항목의 Boolean값을 가져옵니다.
                /// </summary>
                /// <param name="displayName">값을 가져올 항목의 표기이름을 기입하십시오.</param>
                /// <param name="champUniq">이 값을 true로 지정하면 플레이어의 챔피언 이름을 포함한 이름을 가진 항목의 값을 가져옵니다.</param>
                internal static bool GetBoolValue(string displayName, bool champUniq = true)
                {
                    return MenuInstance.Item("Jungleclear." + displayName, champUniq).GetValue<bool>();
                }

                /// <summary>
                ///     Jungleclear 메뉴 안에 존재하는 항목의 LeageuSharp.Common.Slider 구조체를 가져옵니다.
                /// </summary>
                /// <param name="displayName"></param>
                /// <param name="champUniq"></param>
                /// <returns>LeageuSharp.Common.Slider 구조체를 반환합니다.</returns>
                internal static Slider GetSliderValue(string displayName, bool champUniq = true)
                {
                    return MenuInstance.Item("Jungleclear." + displayName, champUniq).GetValue<Slider>();
                }

                /// <summary>
                ///     Jungleclear 메뉴에 'Use Q' 항목을 추가합니다..
                /// </summary>
                /// <param name="enabled">항목의 초기값을 기입하십시오.</param>
                internal static void AddUseQ(bool enabled = true)
                {
                    AddItem("Use Q", enabled);
                }

                /// <summary>
                ///     Jungleclear 메뉴에 'Use W' 항목을 추가합니다..
                /// </summary>
                /// <param name="enabled">항목의 초기값을 기입하십시오.</param>
                internal static void AddUseW(bool enabled = true)
                {
                    AddItem("Use W", enabled);
                }

                /// <summary>
                ///     Jungleclear 메뉴에 'Use E' 항목을 추가합니다..
                /// </summary>
                /// <param name="enabled">항목의 초기값을 기입하십시오.</param>
                internal static void AddUseE(bool enabled = true)
                {
                    AddItem("Use E", enabled);
                }

                /// <summary>
                ///     Jungleclear 메뉴에 'Use R' 항목을 추가합니다..
                /// </summary>
                /// <param name="enabled">항목의 초기값을 기입하십시오.</param>
                internal static void AddUseR(bool enabled = true)
                {
                    AddItem("Use R", enabled);
                }

                /// <summary>
                ///     Jungleclear 메뉴에 'If Mana >' 항목을 추가합니다.
                /// </summary>
                /// <param name="defaultValue">항목의 초기값을 기입하십시오.</param>
                internal static void AddIfMana(int defaultValue = 0)
                {
                    AddItem("If Mana >", new Slider(defaultValue));
                }
            }

            /// <summary>
            ///     이 클래스를 이용해서 Flee 메뉴에 접근할 수 있습니다.
            /// </summary>
            internal class Flee
            {
                /// <summary>
                ///     'Use Q' 항목의 값을 반환합니다.
                /// </summary>
                internal static bool UseQ
                {
                    get { return GetBoolValue("Use Q"); }
                }

                /// <summary>
                ///     'Use W' 항목의 값을 반환합니다.
                /// </summary>
                internal static bool UseW
                {
                    get { return GetBoolValue("Use W"); }
                }

                /// <summary>
                ///     'Use E' 항목의 값을 반환합니다.
                /// </summary>
                internal static bool UseE
                {
                    get { return GetBoolValue("Use E"); }
                }

                /// <summary>
                ///     'Use R' 항목의 값을 반환합니다.
                /// </summary>
                internal static bool UseR
                {
                    get { return GetBoolValue("Use R"); }
                }

                /// <summary>
                ///     'If Mana >' 항목의 값을 반환합니다.
                /// </summary>
                internal static int IfMana
                {
                    get { return GetSliderValue("If Mana >").Value; }
                }

                /// <summary>
                ///     Flee 메뉴에 항목을 추가합니다.
                /// </summary>
                /// <param name="displayName">항목의 표기이름을 기입하십시오.</param>
                /// <param name="value">항목의 초기값을 기입하십시오.</param>
                /// <param name="champUniq">이 값을 true로 지정하면 항목의 이름에 플레이어의 챔피언이름을 포함한 항목이 생성됩니다.</param>
                internal static void AddItem(string displayName, object value = null, bool champUniq = true)
                {
                    if (value == null)
                    {
                        MenuInstance.SubMenu("Champion")
                            .SubMenu("Flee")
                            .AddItem(new MenuItem("Flee." + displayName, displayName, champUniq));
                        return;
                    }

                    MenuInstance.SubMenu("Champion")
                        .SubMenu("Flee")
                        .AddItem(new MenuItem("Flee." + displayName, displayName, champUniq))
                        .SetValue(value);
                }

                /// <summary>
                ///     Flee 메뉴 안에 존재하는 항목의 Boolean값을 가져옵니다.
                /// </summary>
                /// <param name="displayName">값을 가져올 항목의 표기이름을 기입하십시오.</param>
                /// <param name="champUniq">이 값을 true로 지정하면 플레이어의 챔피언 이름을 포함한 이름을 가진 항목의 값을 가져옵니다.</param>
                internal static bool GetBoolValue(string displayName, bool champUniq = true)
                {
                    return MenuInstance.Item("Flee." + displayName, champUniq).GetValue<bool>();
                }

                /// <summary>
                ///     Flee 메뉴 안에 존재하는 항목의 LeageuSharp.Common.Slider 구조체를 가져옵니다.
                /// </summary>
                /// <param name="displayName"></param>
                /// <param name="champUniq"></param>
                /// <returns>LeageuSharp.Common.Slider 구조체를 반환합니다.</returns>
                internal static Slider GetSliderValue(string displayName, bool champUniq = true)
                {
                    return MenuInstance.Item("Flee." + displayName, champUniq).GetValue<Slider>();
                }

                /// <summary>
                ///     Flee 메뉴에 'Use Q' 항목을 추가합니다.
                /// </summary>
                /// <param name="enabled">항목의 초기값을 기입하십시오.</param>
                internal static void AddUseQ(bool enabled = true)
                {
                    AddItem("Use Q", enabled);
                }

                /// <summary>
                ///     Flee 메뉴에 'Use W' 항목을 추가합니다.
                /// </summary>
                /// <param name="enabled">항목의 초기값을 기입하십시오.</param>
                internal static void AddUseW(bool enabled = true)
                {
                    AddItem("Use W", enabled);
                }

                /// <summary>
                ///     Flee 메뉴에 'Use E' 항목을 추가합니다.
                /// </summary>
                /// <param name="enabled">항목의 초기값을 기입하십시오.</param>
                internal static void AddUseE(bool enabled = true)
                {
                    AddItem("Use E", enabled);
                }

                /// <summary>
                ///     Flee 메뉴에 'Use R' 항목을 추가합니다.
                /// </summary>
                /// <param name="enabled">항목의 초기값을 기입하십시오.</param>
                internal static void AddUseR(bool enabled = true)
                {
                    AddItem("Use R", enabled);
                }

                /// <summary>
                ///     Flee 메뉴에 'If Mana >' 항목을 추가합니다.
                /// </summary>
                /// <param name="defaultValue">항목의 초기값을 기입하십시오.</param>
                internal static void AddIfMana(int defaultValue = 0)
                {
                    AddItem("If Mana >", new Slider(defaultValue));
                }
            }

            /// <summary>
            ///     이 클래스를 이용해서 Misc 메뉴에 접근할 수 있습니다.
            /// </summary>
            internal class Misc
            {
                /// <summary>
                ///     'Use Anti-Gacploser' 항목의 값을 가져옵니다.
                /// </summary>
                internal static bool UseAntiGapcloser
                {
                    get { return GetBoolValue("Use Anti-Gapcloser"); }
                }

                /// <summary>
                ///     'Use Interrupter' 항목의 값을 가져옵니다.
                /// </summary>
                internal static bool UseInterrupter
                {
                    get { return GetBoolValue("Use Interrupter"); }
                }

                /// <summary>
                ///     'Use Killsteal' 항목의 값을 가져옵니다.
                /// </summary>
                internal static bool UseKillsteal
                {
                    get { return GetBoolValue("Use Killsteal"); }
                }

                internal static HitChance QSelectedHitchance
                {
                    get
                    {
                        switch (GetStringListValue("Q Hitchance").SelectedValue)
                        {
                            case "Low":
                                return HitChance.Low;
                            case "Medium":
                                return HitChance.Medium;
                            case "High":
                                return HitChance.High;
                            case "Very High":
                                return HitChance.VeryHigh;
                            default:
                                return HitChance.High;
                        }
                    }
                }

                internal static HitChance WSelectedHitchance
                {
                    get
                    {
                        switch (GetStringListValue("W Hitchance").SelectedValue)
                        {
                            case "Low":
                                return HitChance.Low;
                            case "Medium":
                                return HitChance.Medium;
                            case "High":
                                return HitChance.High;
                            case "Very High":
                                return HitChance.VeryHigh;
                            default:
                                return HitChance.High;
                        }
                    }
                }

                internal static HitChance ESelectedHitchance
                {
                    get
                    {
                        switch (GetStringListValue("E Hitchance").SelectedValue)
                        {
                            case "Low":
                                return HitChance.Low;
                            case "Medium":
                                return HitChance.Medium;
                            case "High":
                                return HitChance.High;
                            case "Very High":
                                return HitChance.VeryHigh;
                            default:
                                return HitChance.High;
                        }
                    }
                }

                internal static HitChance RSelectedHitchance
                {
                    get
                    {
                        switch (GetStringListValue("R Hitchance").SelectedValue)
                        {
                            case "Low":
                                return HitChance.Low;
                            case "Medium":
                                return HitChance.Medium;
                            case "High":
                                return HitChance.High;
                            case "Very High":
                                return HitChance.VeryHigh;
                            default:
                                return HitChance.High;
                        }
                    }
                }

                /// <summary>
                ///     Misc 메뉴에 항목을 추가합니다.
                /// </summary>
                /// <param name="displayName">항목의 표기이름을 기입하십시오.</param>
                /// <param name="value">항목의 초기값을 기입하십시오.</param>
                /// <param name="champUniq">이 값을 true로 지정하면 항목의 이름에 플레이어의 챔피언이름을 포함한 항목이 생성됩니다.</param>
                internal static void AddItem(string displayName, object value = null, bool champUniq = true)
                {
                    if (value == null)
                    {
                        MenuInstance.SubMenu("Champion")
                            .SubMenu("Misc")
                            .AddItem(new MenuItem("Misc." + displayName, displayName, champUniq));
                        return;
                    }

                    MenuInstance.SubMenu("Champion")
                        .SubMenu("Misc")
                        .AddItem(new MenuItem("Misc." + displayName, displayName, champUniq))
                        .SetValue(value);
                }

                /// <summary>
                ///     Misc 메뉴 안에 존재하는 항목의 Boolean값을 가져옵니다.
                /// </summary>
                /// <param name="displayName">값을 가져올 항목의 표기이름을 기입하십시오.</param>
                /// <param name="champUniq">이 값을 true로 지정하면 플레이어의 챔피언 이름을 포함한 이름을 가진 항목의 값을 가져옵니다.</param>
                internal static bool GetBoolValue(string displayName, bool champUniq = true)
                {
                    return MenuInstance.Item("Misc." + displayName, champUniq).GetValue<bool>();
                }

                /// <summary>
                ///     Misc 메뉴 안에 존재하는 항목의 LeageuSharp.Common.Slider 구조체를 가져옵니다.
                /// </summary>
                /// <param name="displayName"></param>
                /// <param name="champUniq"></param>
                /// <returns>LeageuSharp.Common.Slider 구조체를 반환합니다.</returns>
                internal static Slider GetSliderValue(string displayName, bool champUniq = true)
                {
                    return MenuInstance.Item("Misc." + displayName, champUniq).GetValue<Slider>();
                }

                /// <summary>
                ///     Misc 메뉴 안에 존재하는 항목의 LeageuSharp.Common.StringList 구조체를 가져옵니다.
                /// </summary>
                /// <param name="displayName"></param>
                /// <param name="champUniq"></param>
                /// <returns>LeageuSharp.Common.StringList 구조체를 반환합니다.</returns>
                internal static StringList GetStringListValue(string displayName, bool champUniq = true)
                {
                    return MenuInstance.Item("Misc." + displayName, champUniq).GetValue<StringList>();
                }

                /// <summary>
                ///     Misc 메뉴 안에 존재하는 항목의 LeageuSharp.Common.KeyBind 구조체를 가져옵니다.
                /// </summary>
                /// <param name="displayName"></param>
                /// <param name="champUniq"></param>
                /// <returns>LeageuSharp.Common.KeyBind 구조체를 반환합니다.</returns>
                internal static KeyBind GetKeyBindValue(string displayName, bool champUniq = true)
                {
                    return MenuInstance.Item("Misc." + displayName, champUniq).GetValue<KeyBind>();
                }

                /// <summary>
                ///     Misc 메뉴에 'Use Anti-Gapcloser' 항목을 추가합니다.
                /// </summary>
                /// <param name="enabled">항목의 초기값을 기입하십시오.</param>
                internal static void AddUseAntiGapcloser(bool enabled = true)
                {
                    AddItem("Use Anti-Gapcloser", enabled);
                }

                /// <summary>
                ///     Misc 메뉴에 'Use Interrupter' 항목을 추가합니다.
                /// </summary>
                /// <param name="enabled">항목의 초기값을 기입하십시오.</param>
                internal static void AddUseInterrupter(bool enabled = true)
                {
                    AddItem("Use Interrupter", enabled);
                }

                /// <summary>
                ///     Misc 메뉴에 'Use Killsteal' 항목을 추가합니다.
                /// </summary>
                /// <param name="enabled">항목의 초기값을 기입하십시오.</param>
                internal static void AddUseKillsteal(bool enabled = true)
                {
                    AddItem("Use Killsteal", enabled);
                }

                internal static void AddQHitchanceSelector(HitChance defaultHitchance = HitChance.High)
                {
                    int defaultindex;

                    switch (defaultHitchance)
                    {
                        case HitChance.Low:
                            defaultindex = 0;
                            break;
                        case HitChance.Medium:
                            defaultindex = 1;
                            break;
                        case HitChance.High:
                            defaultindex = 2;
                            break;
                        case HitChance.VeryHigh:
                            defaultindex = 3;
                            break;
                        default:
                            defaultindex = 2;
                            break;
                    }

                    AddItem("Q Hitchance", new StringList(new[] {"Low", "Medium", "High", "Very High"}, defaultindex));
                }

                internal static void AddWHitchanceSelector(HitChance defaultHitchance = HitChance.High)
                {
                    int defaultindex;

                    switch (defaultHitchance)
                    {
                        case HitChance.Low:
                            defaultindex = 0;
                            break;
                        case HitChance.Medium:
                            defaultindex = 1;
                            break;
                        case HitChance.High:
                            defaultindex = 2;
                            break;
                        case HitChance.VeryHigh:
                            defaultindex = 3;
                            break;
                        default:
                            defaultindex = 2;
                            break;
                    }

                    AddItem("W Hitchance", new StringList(new[] {"Low", "Medium", "High", "Very High"}, defaultindex));
                }

                internal static void AddEHitchanceSelector(HitChance defaultHitchance = HitChance.High)
                {
                    int defaultindex;

                    switch (defaultHitchance)
                    {
                        case HitChance.Low:
                            defaultindex = 0;
                            break;
                        case HitChance.Medium:
                            defaultindex = 1;
                            break;
                        case HitChance.High:
                            defaultindex = 2;
                            break;
                        case HitChance.VeryHigh:
                            defaultindex = 3;
                            break;
                        default:
                            defaultindex = 2;
                            break;
                    }

                    AddItem("E Hitchance", new StringList(new[] {"Low", "Medium", "High", "Very High"}, defaultindex));
                }

                internal static void AddRHitchanceSelector(HitChance defaultHitchance = HitChance.High)
                {
                    int defaultindex;

                    switch (defaultHitchance)
                    {
                        case HitChance.Low:
                            defaultindex = 0;
                            break;
                        case HitChance.Medium:
                            defaultindex = 1;
                            break;
                        case HitChance.High:
                            defaultindex = 2;
                            break;
                        case HitChance.VeryHigh:
                            defaultindex = 3;
                            break;
                        default:
                            defaultindex = 2;
                            break;
                    }

                    AddItem("R Hitchance", new StringList(new[] {"Low", "Medium", "High", "Very High"}, defaultindex));
                }
            }

            /// <summary>
            ///     이 클래스를 이용해서 Drawings 메뉴에 접근할 수 있습니다.
            /// </summary>
            internal class Drawings
            {
                /// <summary>
                ///     'Q Range' 항목의 Circle 구조체를 반환합니다.
                /// </summary>
                internal static Circle DrawQrange
                {
                    get { return GetCircleValue("Draw Q Range"); }
                }

                /// <summary>
                ///     'W Range' 항목의 Circle 구조체를 반환합니다.
                /// </summary>
                internal static Circle DrawWrange
                {
                    get { return GetCircleValue("Draw W Range"); }
                }

                /// <summary>
                ///     'E Range' 항목의 Circle 구조체를 반환합니다.
                /// </summary>
                internal static Circle DrawErange
                {
                    get { return GetCircleValue("Draw E Range"); }
                }

                /// <summary>
                ///     'R Range' 항목의 Circle 구조체를 반환합니다.
                /// </summary>
                internal static Circle DrawRrange
                {
                    get { return GetCircleValue("Draw R Range"); }
                }

                /// <summary>
                ///     Drawings 메뉴에 항목을 추가합니다.
                /// </summary>
                /// <param name="displayName">항목의 표기이름을 기입하십시오.</param>
                /// <param name="value">항목의 초기값을 기입하십시오.</param>
                /// <param name="champUniq">이 값을 true로 지정하면 항목의 이름에 플레이어의 챔피언이름을 포함한 항목이 생성됩니다.</param>
                internal static void AddItem(string displayName, object value = null, bool champUniq = true)
                {
                    if (value == null)
                    {
                        MenuInstance.SubMenu("Champion")
                            .SubMenu("Drawings")
                            .AddItem(new MenuItem("Drawings." + displayName, displayName, champUniq));
                        return;
                    }

                    MenuInstance.SubMenu("Champion")
                        .SubMenu("Drawings")
                        .AddItem(new MenuItem("Drawings." + displayName, displayName, champUniq))
                        .SetValue(value);
                }

                /// <summary>
                ///     Drawings 메뉴 안에 존재하는 항목의 Boolean값을 가져옵니다.
                /// </summary>
                /// <param name="displayName">값을 가져올 항목의 표기이름을 기입하십시오.</param>
                /// <param name="champUniq">이 값을 true로 지정하면 플레이어의 챔피언 이름을 포함한 이름을 가진 항목의 값을 가져옵니다.</param>
                internal static bool GetBoolValue(string displayName, bool champUniq = true)
                {
                    return MenuInstance.Item("Drawings." + displayName, champUniq).GetValue<bool>();
                }

                /// <summary>
                ///     Drawings 메뉴 안에 존재하는 항목의 LeageuSharp.Common.Slider 구조체를 가져옵니다.
                /// </summary>
                /// <param name="displayName"></param>
                /// <param name="champUniq"></param>
                /// <returns>LeageuSharp.Common.Slider 구조체를 반환합니다.</returns>
                internal static Slider GetSliderValue(string displayName, bool champUniq = true)
                {
                    return MenuInstance.Item("Drawings." + displayName, champUniq).GetValue<Slider>();
                }

                /// <summary>
                ///     Drawings 메뉴 안에 존재하는 항목의 LeagueSharp.Common.Circle 구조체를 가져옵니다.
                /// </summary>
                /// <param name="displayName"></param>
                /// <param name="champUniq"></param>
                /// <returns>LeagueSharp.Common.Circle 구조체를 반환합니다.</returns>
                internal static Circle GetCircleValue(string displayName, bool champUniq = true)
                {
                    return MenuInstance.Item("Drawings." + displayName, champUniq).GetValue<Circle>();
                }

                /// <summary>
                ///     Drawings 메뉴에 'Q Range' 항목을 추가합니다.
                /// </summary>
                /// <param name="enabled">항목의 초기값을 기입하십시오.</param>
                internal static void AddDrawQrange(Color color, bool enabled = true)
                {
                    AddItem("Draw Q Range", new Circle(enabled, color));
                }

                /// <summary>
                ///     Drawings 메뉴에 'W Range' 항목을 추가합니다.
                /// </summary>
                /// <param name="enabled">항목의 초기값을 기입하십시오.</param>
                internal static void AddDrawWrange(Color color, bool enabled = true)
                {
                    AddItem("Draw W Range", new Circle(enabled, color));
                }

                /// <summary>
                ///     Drawings 메뉴에 'E Range' 항목을 추가합니다.
                /// </summary>
                /// <param name="enabled">항목의 초기값을 기입하십시오.</param>
                internal static void AddDrawErange(Color color, bool enabled = true)
                {
                    AddItem("Draw E Range", new Circle(enabled, color));
                }

                /// <summary>
                ///     Drawings 메뉴에 'R Range' 항목을 추가합니다.
                /// </summary>
                /// <param name="enabled">항목의 초기값을 기입하십시오.</param>
                internal static void AddDrawRrange(Color color, bool enabled = true)
                {
                    AddItem("Draw R Range", new Circle(enabled, color));
                }

                /// <summary>
                ///     Drawings 메뉴에 DamageIndicator(데미지 표시기)를 추가합니다.
                /// </summary>
                /// <param name="damage"></param>
                internal static void AddDamageIndicator(DamageIndicator.DamageToUnitDelegate damage)
                {
                    var drawDamageMenu = new MenuItem("Draw_DamageIndicator", "DamageIndicator", true).SetValue(true);
                    var drawDamageFill =
                        new MenuItem("DamageIndicator_FillColor", "DamageIndicator FillColor", true).SetValue(
                            new Circle(true, Color.Goldenrod));

                    MenuInstance.SubMenu("Champion").SubMenu("Drawings").AddItem(drawDamageMenu);
                    MenuInstance.SubMenu("Champion").SubMenu("Drawings").AddItem(drawDamageFill);

                    DamageIndicator.DamageToUnit = damage;
                    DamageIndicator.Enabled = drawDamageMenu.GetValue<bool>();
                    DamageIndicator.Fill = drawDamageFill.GetValue<Circle>().Active;
                    DamageIndicator.FillColor = drawDamageFill.GetValue<Circle>().Color;

                    drawDamageMenu.ValueChanged +=
                        delegate(object sender, OnValueChangeEventArgs eventArgs)
                        {
                            DamageIndicator.Enabled = eventArgs.GetNewValue<bool>();
                        };

                    drawDamageFill.ValueChanged +=
                        delegate(object sender, OnValueChangeEventArgs eventArgs)
                        {
                            DamageIndicator.Fill = eventArgs.GetNewValue<Circle>().Active;
                            DamageIndicator.FillColor = eventArgs.GetNewValue<Circle>().Color;
                        };
                }

                /// <summary>
                ///     Drawings 메뉴에 DamageIndicatorForJungle(정글몹에 데미지 표시)를 추가합니다.
                /// </summary>
                /// <param name="damage"></param>
                internal static void AddDamageIndicatorForJungle(DamageIndicatorForJungle.DamageToUnitDelegate damage)
                {
                    var drawDamageMenu =
                        new MenuItem("Draw_DamageIndicatorForJungle", "DamageIndicator For Jungle", true).SetValue(true);
                    var drawDamageFill =
                        new MenuItem("DamageIndicatorForJungle_FillColor", "DamageIndicator For Jungle FillColor", true)
                            .SetValue(new Circle(true, Color.GreenYellow));

                    MenuInstance.SubMenu("Champion").SubMenu("Drawings").AddItem(drawDamageMenu);
                    MenuInstance.SubMenu("Champion").SubMenu("Drawings").AddItem(drawDamageFill);

                    DamageIndicatorForJungle.DamageToUnit = damage;
                    DamageIndicatorForJungle.Enabled = drawDamageMenu.GetValue<bool>();
                    DamageIndicatorForJungle.Fill = drawDamageFill.GetValue<Circle>().Active;
                    DamageIndicatorForJungle.FillColor = drawDamageFill.GetValue<Circle>().Color;

                    drawDamageMenu.ValueChanged +=
                        delegate(object sender, OnValueChangeEventArgs eventArgs)
                        {
                            DamageIndicatorForJungle.Enabled = eventArgs.GetNewValue<bool>();
                        };

                    drawDamageFill.ValueChanged +=
                        delegate(object sender, OnValueChangeEventArgs eventArgs)
                        {
                            DamageIndicatorForJungle.Fill = eventArgs.GetNewValue<Circle>().Active;
                            DamageIndicatorForJungle.FillColor = eventArgs.GetNewValue<Circle>().Color;
                        };
                }
            }
        }
    }
}