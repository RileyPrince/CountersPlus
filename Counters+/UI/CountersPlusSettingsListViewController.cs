﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using CustomUI.BeatSaber;
using VRUI;
using HMUI;
using CountersPlus.Config;
using IniParser.Model;
using IniParser;
using CountersPlus.Custom;
using IllusionInjector;
using IllusionPlugin;
using System.Collections;

namespace CountersPlus.UI
{
    class CountersPlusSettingsListViewController : CustomListViewController
    {
        private LevelListTableCell cellInstance;
        public List<string> test = new List<string>() { "wat" };

        //public Dictionary<IConfigModel, SettingsInfo> counterInfos = new Dictionary<IConfigModel, SettingsInfo>() { };

        public List<SettingsInfo> counterInfos = new List<SettingsInfo>();

        protected override void DidActivate(bool firstActivation, ActivationType type)
        {
            try
            {
                if (firstActivation)
                {
                    
                    cellInstance = Resources.FindObjectsOfTypeAll<LevelListTableCell>().First((LevelListTableCell x) => x.name == "LevelListTableCell");
                    base.DidActivate(firstActivation, type);

                    foreach (var kvp in CountersSettingsUI.counterUIItems)
                    {
                        //counterInfos.Add(kvp.Key, CreateFromModel(kvp.Key));
                        counterInfos.Add(CreateFromModel(kvp.Key));
                    }
                    FileIniDataParser parser = new FileIniDataParser();
                    IniData data = parser.ReadFile(Environment.CurrentDirectory.Replace('\\', '/') + "/UserData/CountersPlus.ini");
                    foreach (SectionData section in data.Sections)
                    {
                        if (section.Keys.Any((KeyData x) => x.KeyName == "SectionName"))
                        {
                            CustomConfigModel potential = new CustomConfigModel(section.SectionName);
                            if (!PluginManager.Plugins.Any((IPlugin x) => x.Name == section.Keys["ModCreator"])) continue;
                            counterInfos.Add(new SettingsInfo()
                            {
                                Name = potential.DisplayName,
                                Description = $"A custom counter added by {potential.ModCreator}!",
                            });
                        }
                    }
                    _customListTableView.didSelectRowEvent += onCellSelect;
                    _customListTableView.ReloadData();
                    StartCoroutine(ApplyEditViewControllerSettings());
                }
            }
            catch (Exception e) {
                Plugin.Log(e.ToString(), Plugin.LogInfo.Fatal);
            }
        }

        IEnumerator ApplyEditViewControllerSettings()
        {
            yield return new WaitUntil(() => CountersPlusEditViewController.Instance != null);
            CountersPlusEditViewController.Instance.didFinishEvent += applySettings;
            Plugin.Log("We should be good to go!");
        }

        private SettingsInfo CreateFromModel<T>(T settings) where T : IConfigModel
        {
            SettingsInfo info = new SettingsInfo()
            {
                Name = settings.DisplayName,
                Description = DescriptionForModel(settings),
            };
            return info;
        }

        private string DescriptionForModel<T>(T settings) where T : IConfigModel
        {
            switch (settings.DisplayName)
            {
                default:
                    return "Huh, I dont know, I cant find this!";
                case "Missed":
                    return "<i>MISS</i>";
                case "Notes":
                    return "Notes hit over total notes!";
                case "Progress":
                    return "The original you know and love.";
                case "Score":
                    return "If the in-game score counter wasn't good enough.";
                case "Speed":
                    return "<i>\"Speed, motherfucker, do you speak it?\"</i>";
                case "Cut":
                    return "How well you hit those bloqs.";
            }
        }

        public override float RowHeight()
        {
            return 10f;
        }

        public override int NumberOfRows()
        {
            return counterInfos.Count + 1;
        }

        public override TableCell CellForRow(int row)
        {
            LevelListTableCell cell = Instantiate(cellInstance);
            if (row == 0)
            {
                cell.songName = "Main Settings";
                cell.author = "Configure basic Counters+ settings.";
            }
            else
            {
                SettingsInfo info = counterInfos[row - 1];
                cell.songName = info.Name;
                cell.author = info.Description;
            }
            cell.coverImage = Sprite.Create(Texture2D.blackTexture, default(Rect), Vector2.zero);
            cell.reuseIdentifier = "CountersPlusSettingCell";
            return cell;
        }

        private void onCellSelect(TableView view, int row)
        {
            Plugin.Log("Lets obtain some settings!");
        }

        private void applySettings()
        {
            Plugin.Log("Settings would apply here!");
        }
    }

    class SettingsInfo{
        public string Name;
        public string Description;
    }
}