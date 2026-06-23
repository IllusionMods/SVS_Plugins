using ADV;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Manager;
using SV;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace MapLoader
{
    internal class MapLoaderUtils
    {
        public static int GetTimeZone()
        {
            var nowMode = SimulationManager.Instance.Mode;

            switch (nowMode)
            {
                case SimulationManager.SimulationMode.SimMorning:
                    return 0;
                case SimulationManager.SimulationMode.SimNoon:
                    return 1;
                case SimulationManager.SimulationMode.SimEvening:
                    return 2;
                case SimulationManager.SimulationMode.SimNight:
                    return 3;
            }
            return -1;
        }

        public static string GetWeekDay()
        {
            int weekDay = Manager.Game.saveData.Week;
            string day = "Monday";
            switch (weekDay)
            {
                case 0:
                    day = "Monday";
                    break;
                case 1:
                    day = "Tuesday";
                    break;
                case 2:
                    day = "Wesnesday";
                    break;
                case 3:
                    day = "Thursday";
                    break;
                case 4:
                    day = "Friday";
                    break;
                case 5:
                    day = "Saturday";
                    break;
                case 6:
                    day = "Sunday";
                    break;
            }
            return day;
        }
        public static void InsertJobADV(OpenData openData, Dictionary<string,List<MapLoaderParam.JobADV>> jobsADVs)
        {
            if (openData == null || jobsADVs.Count == 0) return;

            Il2CppReferenceArray<ScenarioCommand> scenarioData = new Il2CppReferenceArray<ScenarioCommand>(0);
            Il2CppReferenceArray<ScenarioCommand> tempScenario = new Il2CppReferenceArray<ScenarioCommand>(0);

            int jobsNum = 5;
            int scenarioLenght = 0;
            int index = 0;
            int newJobsIndex = 0;
            int pos = 0;
            int maxLenght = 0;
            int startAdding = openData._data._list.Count - 3;
            
            int jobsIndex = 0;
            foreach (var sce in openData._data._list)
            {
                if (sce._command == Command.Switch)
                {
                    if (sce._args[0] == "PC_Job" || sce._args[0] == "NPC_Job") break;
                }
                jobsIndex++;
            }

            switch (openData.Asset)
            {
                case "s_86":
                    if (!jobsADVs.ContainsKey("s_86"))
                    {
                        MapLoaderPlugin.Log.LogInfo("ADV s_86 not found");
                        break;
                    } 
                    if (jobsADVs["s_86"].Count == 0) break;

                    jobsNum += jobsADVs["s_86"].Count;
                    openData._data._list[jobsIndex]._args = new Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppStringArray(jobsNum);
                    openData._data._list[jobsIndex]._args[0] = "PC_Job";
                    openData._data._list[jobsIndex]._args[1] = "0,職なし";
                    openData._data._list[jobsIndex]._args[2] = "1,ビーチ監視員";
                    openData._data._list[jobsIndex]._args[3] = "2,カフェ店員";
                    openData._data._list[jobsIndex]._args[4] = "3,巫女・男巫";

                    scenarioLenght = 0;
                    foreach (var scenario in jobsADVs["s_86"])
                    {
                        scenarioLenght += scenario.ScenarioParams.Count;
                    }
                    if (scenarioLenght == 0)
                    {
                        MapLoaderPlugin.Log.LogInfo($"ERROR!: No new scenarios");
                        return;
                    }

                    scenarioData = new Il2CppReferenceArray<ScenarioCommand>(scenarioLenght);

                    index = 0;
                    newJobsIndex = 5;
                    foreach (var jobAdv in jobsADVs["s_86"])
                    {
                        openData._data._list[jobsIndex]._args[newJobsIndex] = jobAdv.JobNameID;

                        foreach (var scenarios in jobAdv.ScenarioParams)
                        {
                            scenarioData[index] = new ScenarioCommand();
                            scenarioData[index]._version = scenarios.Version;
                            scenarioData[index]._multi = scenarios.Multi;
                            scenarioData[index]._command = (Command)Enum.Parse(typeof(Command), scenarios.Command);
                            scenarioData[index]._args = new Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppStringArray(scenarios.Args.Length);
                            for (int i = 0; i < scenarios.Args.Length; i++)
                            {
                                scenarioData[index]._args[i] = scenarios.Args[i];
                            }
                            scenarioData[index].Hash = scenarioData[index].GetHashCode();
                            index++;
                        }
                        newJobsIndex++;
                    }

                    maxLenght = openData._data._list.Count + scenarioLenght;
                    tempScenario = new Il2CppReferenceArray<ScenarioCommand>(maxLenght);

                    pos = 0;
                    for (int i = 0; i < (tempScenario.Count - 2); i++)
                    {
                        if (i > (startAdding)) //109
                        {
                            tempScenario[i] = new ScenarioCommand();
                            tempScenario[i]._version = scenarioData[pos]._version;
                            tempScenario[i]._multi = scenarioData[pos]._multi;
                            tempScenario[i]._command = scenarioData[pos]._command;
                            tempScenario[i]._args = scenarioData[pos]._args;
                            tempScenario[i].Hash = tempScenario[i].GetHashCode();
                            pos++;
                        }
                        else
                        {
                            tempScenario[i] = new ScenarioCommand();
                            tempScenario[i]._version = openData._data._list[i]._version;
                            tempScenario[i]._multi = openData._data._list[i]._multi;
                            tempScenario[i]._command = openData._data._list[i]._command;
                            tempScenario[i]._args = openData._data._list[i]._args;
                            tempScenario[i].Hash = tempScenario[i].GetHashCode();
                        }
                    }

                    tempScenario[maxLenght - 2] = new ScenarioCommand();
                    tempScenario[maxLenght - 2]._version = 0;
                    tempScenario[maxLenght - 2]._multi = false;
                    tempScenario[maxLenght - 2]._command = Command.Tag;
                    tempScenario[maxLenght - 2]._args = new Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppStringArray(1);
                    tempScenario[maxLenght - 2]._args[0] = "END";
                    tempScenario[maxLenght - 2].Hash = tempScenario[maxLenght - 2].GetHashCode();

                    tempScenario[maxLenght - 1] = new ScenarioCommand();
                    tempScenario[maxLenght - 1]._version = 0;
                    tempScenario[maxLenght - 1]._multi = false;
                    tempScenario[maxLenght - 1]._command = Command.Close;
                    //tempScenario[maxLenght - 1]._args = new Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppStringArray(0);
                    tempScenario[maxLenght - 1].Hash = tempScenario[maxLenght - 1].GetHashCode();

                    openData._data._list = tempScenario;
                    break;

                case "a_25_1":
                    if (!jobsADVs.ContainsKey("a_25_1"))
                    {
                        MapLoaderPlugin.Log.LogInfo("ADV a_25_1 not found");
                        break;
                    }
                    if (jobsADVs["a_25_1"].Count == 0) break;

                    jobsNum += jobsADVs["a_25_1"].Count;
                    openData._data._list[jobsIndex]._args = new Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppStringArray(jobsNum);
                    openData._data._list[jobsIndex]._args[0] = "PC_Job";
                    openData._data._list[jobsIndex]._args[1] = "0,職なし";
                    openData._data._list[jobsIndex]._args[2] = "1,ビーチ監視員";
                    openData._data._list[jobsIndex]._args[3] = "2,カフェ店員";
                    openData._data._list[jobsIndex]._args[4] = "3,巫女・男巫";

                    scenarioLenght = 0;
                    foreach (var scenario in jobsADVs["a_25_1"])
                    {
                        scenarioLenght += scenario.ScenarioParams.Count;
                    }
                    if (scenarioLenght == 0)
                    {
                        MapLoaderPlugin.Log.LogInfo($"ERROR!: No new scenarios");
                        return;
                    }

                    scenarioData = new Il2CppReferenceArray<ScenarioCommand>(scenarioLenght);

                    index = 0;
                    newJobsIndex = 5;
                    foreach (var jobAdv in jobsADVs["a_25_1"])
                    {
                        openData._data._list[jobsIndex]._args[newJobsIndex] = jobAdv.JobNameID;

                        foreach (var scenarios in jobAdv.ScenarioParams)
                        {
                            scenarioData[index] = new ScenarioCommand();
                            scenarioData[index]._version = scenarios.Version;
                            scenarioData[index]._multi = scenarios.Multi;
                            scenarioData[index]._command = (Command)Enum.Parse(typeof(Command), scenarios.Command);
                            scenarioData[index]._args = new Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppStringArray(scenarios.Args.Length);
                            for (int i = 0; i < scenarios.Args.Length; i++)
                            {
                                scenarioData[index]._args[i] = scenarios.Args[i];
                            }
                            scenarioData[index].Hash = scenarioData[index].GetHashCode();
                            index++;
                        }
                        newJobsIndex++;
                    }

                    maxLenght = openData._data._list.Count + scenarioLenght;
                    tempScenario = new Il2CppReferenceArray<ScenarioCommand>(maxLenght);

                    pos = 0;
                    for (int i = 0; i < (tempScenario.Count - 2); i++)
                    {
                        if (i > startAdding)
                        {
                            tempScenario[i] = new ScenarioCommand();
                            tempScenario[i]._version = scenarioData[pos]._version;
                            tempScenario[i]._multi = scenarioData[pos]._multi;
                            tempScenario[i]._command = scenarioData[pos]._command;
                            tempScenario[i]._args = scenarioData[pos]._args;
                            tempScenario[i].Hash = tempScenario[i].GetHashCode();
                            pos++;
                        }
                        else
                        {
                            tempScenario[i] = new ScenarioCommand();
                            tempScenario[i]._version = openData._data._list[i]._version;
                            tempScenario[i]._multi = openData._data._list[i]._multi;
                            tempScenario[i]._command = openData._data._list[i]._command;
                            tempScenario[i]._args = openData._data._list[i]._args;
                            tempScenario[i].Hash = tempScenario[i].GetHashCode();
                        }
                    }

                    tempScenario[maxLenght - 2] = new ScenarioCommand();
                    tempScenario[maxLenght - 2]._version = 0;
                    tempScenario[maxLenght - 2]._multi = false;
                    tempScenario[maxLenght - 2]._command = Command.Tag;
                    tempScenario[maxLenght - 2]._args = new Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppStringArray(1);
                    tempScenario[maxLenght - 2]._args[0] = "END";

                    tempScenario[maxLenght - 1] = new ScenarioCommand();
                    tempScenario[maxLenght - 1]._version = 0;
                    tempScenario[maxLenght - 1]._multi = false;
                    tempScenario[maxLenght - 1]._command = Command.Close;

                    openData._data._list = tempScenario;

                    break;

                case "p_25_1":
                    if (!jobsADVs.ContainsKey("p_25_1"))
                    {
                        MapLoaderPlugin.Log.LogInfo("ADV p_25_1 not found");
                        break;
                    }
                    if (jobsADVs["p_25_1"].Count == 0) break;

                    jobsNum += jobsADVs["p_25_1"].Count;
                    openData._data._list[jobsIndex]._args = new Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppStringArray(jobsNum);
                    openData._data._list[jobsIndex]._args[0] = "NPC_Job";
                    openData._data._list[jobsIndex]._args[1] = "0,職なし";
                    openData._data._list[jobsIndex]._args[2] = "1,ビーチ監視員";
                    openData._data._list[jobsIndex]._args[3] = "2,カフェ店員";
                    openData._data._list[jobsIndex]._args[4] = "3,巫女・男巫";

                    scenarioLenght = 0;
                    foreach (var scenario in jobsADVs["p_25_1"])
                    {
                        scenarioLenght += scenario.ScenarioParams.Count;
                    }
                    if (scenarioLenght == 0)
                    {
                        MapLoaderPlugin.Log.LogInfo($"ERROR!: No new scenarios");
                        return;
                    }

                    scenarioData = new Il2CppReferenceArray<ScenarioCommand>(scenarioLenght);

                    index = 0;
                    newJobsIndex = 5;
                    foreach (var jobAdv in jobsADVs["p_25_1"])
                    {
                        openData._data._list[jobsIndex]._args[newJobsIndex] = jobAdv.JobNameID;

                        foreach (var scenarios in jobAdv.ScenarioParams)
                        {
                            scenarioData[index] = new ScenarioCommand();
                            scenarioData[index]._version = scenarios.Version;
                            scenarioData[index]._multi = scenarios.Multi;
                            scenarioData[index]._command = (Command)Enum.Parse(typeof(Command), scenarios.Command);
                            scenarioData[index]._args = new Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppStringArray(scenarios.Args.Length);
                            for (int i = 0; i < scenarios.Args.Length; i++)
                            {
                                scenarioData[index]._args[i] = scenarios.Args[i];
                            }
                            scenarioData[index].Hash = scenarioData[index].GetHashCode();
                            index++;
                        }
                        newJobsIndex++;
                    }

                    maxLenght = openData._data._list.Count + scenarioLenght;
                    tempScenario = new Il2CppReferenceArray<ScenarioCommand>(maxLenght);

                    pos = 0;
                    for (int i = 0; i < (tempScenario.Count - 2); i++)
                    {
                        if (i > startAdding) //124
                        {
                            tempScenario[i] = new ScenarioCommand();
                            tempScenario[i]._version = scenarioData[pos]._version;
                            tempScenario[i]._multi = scenarioData[pos]._multi;
                            tempScenario[i]._command = scenarioData[pos]._command;
                            tempScenario[i]._args = scenarioData[pos]._args;
                            tempScenario[i].Hash = tempScenario[i].GetHashCode();
                            pos++;
                        }
                        else
                        {
                            tempScenario[i] = new ScenarioCommand();
                            tempScenario[i]._version = openData._data._list[i]._version;
                            tempScenario[i]._multi = openData._data._list[i]._multi;
                            tempScenario[i]._command = openData._data._list[i]._command;
                            tempScenario[i]._args = openData._data._list[i]._args;
                            tempScenario[i].Hash = tempScenario[i].GetHashCode();
                        }
                    }

                    tempScenario[maxLenght - 2] = new ScenarioCommand();
                    tempScenario[maxLenght - 2]._version = 0;
                    tempScenario[maxLenght - 2]._multi = false;
                    tempScenario[maxLenght - 2]._command = Command.Tag;
                    tempScenario[maxLenght - 2]._args = new Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppStringArray(1);
                    tempScenario[maxLenght - 2]._args[0] = "END";

                    tempScenario[maxLenght - 1] = new ScenarioCommand();
                    tempScenario[maxLenght - 1]._version = 0;
                    tempScenario[maxLenght - 1]._multi = false;
                    tempScenario[maxLenght - 1]._command = Command.Close;

                    openData._data._list = tempScenario;

                    break;

                case "a_43_1":
                    if (!jobsADVs.ContainsKey("a_43_1"))
                    {
                        MapLoaderPlugin.Log.LogInfo("ADV a_43_1 not found");
                        break;
                    }
                    if (jobsADVs["a_43_1"].Count == 0) break;

                    jobsNum += jobsADVs["a_43_1"].Count;
                    openData._data._list[jobsIndex]._args = new Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppStringArray(jobsNum);
                    openData._data._list[jobsIndex]._args[0] = "PC_Job";
                    openData._data._list[jobsIndex]._args[1] = "0,職なし";
                    openData._data._list[jobsIndex]._args[2] = "1,ビーチ監視員";
                    openData._data._list[jobsIndex]._args[3] = "2,カフェ店員";
                    openData._data._list[jobsIndex]._args[4] = "3,巫女・男巫";

                    scenarioLenght = 0;
                    foreach (var scenario in jobsADVs["a_43_1"])
                    {
                        scenarioLenght += scenario.ScenarioParams.Count;
                    }
                    if (scenarioLenght == 0)
                    {
                        MapLoaderPlugin.Log.LogInfo($"ERROR!: No new scenarios");
                        return;
                    }

                    scenarioData = new Il2CppReferenceArray<ScenarioCommand>(scenarioLenght);

                    index = 0;
                    newJobsIndex = 5;
                    foreach (var jobAdv in jobsADVs["a_43_1"])
                    {
                        openData._data._list[jobsIndex]._args[newJobsIndex] = jobAdv.JobNameID;

                        foreach (var scenarios in jobAdv.ScenarioParams)
                        {
                            scenarioData[index] = new ScenarioCommand();
                            scenarioData[index]._version = scenarios.Version;
                            scenarioData[index]._multi = scenarios.Multi;
                            scenarioData[index]._command = (Command)Enum.Parse(typeof(Command), scenarios.Command);
                            scenarioData[index]._args = new Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppStringArray(scenarios.Args.Length);
                            for (int i = 0; i < scenarios.Args.Length; i++)
                            {
                                scenarioData[index]._args[i] = scenarios.Args[i];
                            }
                            scenarioData[index].Hash = scenarioData[index].GetHashCode();
                            index++;
                        }
                        newJobsIndex++;
                    }

                    maxLenght = openData._data._list.Count + scenarioLenght;
                    tempScenario = new Il2CppReferenceArray<ScenarioCommand>(maxLenght);

                    pos = 0;
                    for (int i = 0; i < (tempScenario.Count - 2); i++)
                    {
                        if (i > startAdding) //342
                        {
                            tempScenario[i] = new ScenarioCommand();
                            tempScenario[i]._version = scenarioData[pos]._version;
                            tempScenario[i]._multi = scenarioData[pos]._multi;
                            tempScenario[i]._command = scenarioData[pos]._command;
                            tempScenario[i]._args = scenarioData[pos]._args;
                            tempScenario[i].Hash = tempScenario[i].GetHashCode();
                            pos++;
                        }
                        else
                        {
                            tempScenario[i] = new ScenarioCommand();
                            tempScenario[i]._version = openData._data._list[i]._version;
                            tempScenario[i]._multi = openData._data._list[i]._multi;
                            tempScenario[i]._command = openData._data._list[i]._command;
                            tempScenario[i]._args = openData._data._list[i]._args;
                            tempScenario[i].Hash = tempScenario[i].GetHashCode();
                        }
                    }

                    tempScenario[maxLenght - 2] = new ScenarioCommand();
                    tempScenario[maxLenght - 2]._version = 0;
                    tempScenario[maxLenght - 2]._multi = false;
                    tempScenario[maxLenght - 2]._command = Command.Tag;
                    tempScenario[maxLenght - 2]._args = new Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppStringArray(1);
                    tempScenario[maxLenght - 2]._args[0] = "END";

                    tempScenario[maxLenght - 1] = new ScenarioCommand();
                    tempScenario[maxLenght - 1]._version = 0;
                    tempScenario[maxLenght - 1]._multi = false;
                    tempScenario[maxLenght - 1]._command = Command.Close;

                    openData._data._list = tempScenario;
                    break;
                case "p_43_1":
                    if (!jobsADVs.ContainsKey("p_43_1"))
                    {
                        MapLoaderPlugin.Log.LogInfo("ADV p_43_1 not found");
                        break;
                    }
                    if (jobsADVs["p_43_1"].Count == 0) break;

                    jobsNum += jobsADVs["p_43_1"].Count;
                    openData._data._list[jobsIndex]._args = new Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppStringArray(jobsNum);
                    openData._data._list[jobsIndex]._args[0] = "NPC_Job";
                    openData._data._list[jobsIndex]._args[1] = "0,職なし";
                    openData._data._list[jobsIndex]._args[2] = "1,ビーチ監視員";
                    openData._data._list[jobsIndex]._args[3] = "2,カフェ店員";
                    openData._data._list[jobsIndex]._args[4] = "3,巫女・男巫";

                    scenarioLenght = 0;
                    foreach (var scenario in jobsADVs["p_43_1"])
                    {
                        scenarioLenght += scenario.ScenarioParams.Count;
                    }
                    if (scenarioLenght == 0)
                    {
                        MapLoaderPlugin.Log.LogInfo($"ERROR!: No new scenarios");
                        return;
                    }

                    scenarioData = new Il2CppReferenceArray<ScenarioCommand>(scenarioLenght);

                    index = 0;
                    newJobsIndex = 5;
                    foreach (var jobAdv in jobsADVs["p_43_1"])
                    {
                        openData._data._list[jobsIndex]._args[newJobsIndex] = jobAdv.JobNameID;

                        foreach (var scenarios in jobAdv.ScenarioParams)
                        {
                            scenarioData[index] = new ScenarioCommand();
                            scenarioData[index]._version = scenarios.Version;
                            scenarioData[index]._multi = scenarios.Multi;
                            scenarioData[index]._command = (Command)Enum.Parse(typeof(Command), scenarios.Command);
                            scenarioData[index]._args = new Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppStringArray(scenarios.Args.Length);
                            for (int i = 0; i < scenarios.Args.Length; i++)
                            {
                                scenarioData[index]._args[i] = scenarios.Args[i];
                            }
                            scenarioData[index].Hash = scenarioData[index].GetHashCode();
                            index++;
                        }
                        newJobsIndex++;
                    }

                    maxLenght = openData._data._list.Count + scenarioLenght;
                    tempScenario = new Il2CppReferenceArray<ScenarioCommand>(maxLenght);

                    pos = 0;
                    for (int i = 0; i < (tempScenario.Count - 2); i++)
                    {
                        if (i > startAdding)
                        {
                            tempScenario[i] = new ScenarioCommand();
                            tempScenario[i]._version = scenarioData[pos]._version;
                            tempScenario[i]._multi = scenarioData[pos]._multi;
                            tempScenario[i]._command = scenarioData[pos]._command;
                            tempScenario[i]._args = scenarioData[pos]._args;
                            tempScenario[i].Hash = tempScenario[i].GetHashCode();
                            pos++;
                        }
                        else
                        {
                            tempScenario[i] = new ScenarioCommand();
                            tempScenario[i]._version = openData._data._list[i]._version;
                            tempScenario[i]._multi = openData._data._list[i]._multi;
                            tempScenario[i]._command = openData._data._list[i]._command;
                            tempScenario[i]._args = openData._data._list[i]._args;
                            tempScenario[i].Hash = tempScenario[i].GetHashCode();
                        }
                    }

                    tempScenario[maxLenght - 2] = new ScenarioCommand();
                    tempScenario[maxLenght - 2]._version = 0;
                    tempScenario[maxLenght - 2]._multi = false;
                    tempScenario[maxLenght - 2]._command = Command.Tag;
                    tempScenario[maxLenght - 2]._args = new Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppStringArray(1);
                    tempScenario[maxLenght - 2]._args[0] = "END";

                    tempScenario[maxLenght - 1] = new ScenarioCommand();
                    tempScenario[maxLenght - 1]._version = 0;
                    tempScenario[maxLenght - 1]._multi = false;
                    tempScenario[maxLenght - 1]._command = Command.Close;

                    openData._data._list = tempScenario;
                    break;
                case "timechange":
                    if (!jobsADVs.ContainsKey("timechange"))
                    {
                        MapLoaderPlugin.Log.LogInfo("ADV timechange not found");
                        break;
                    }
                    if (jobsADVs["timechange"].Count == 0) break;

                    jobsNum += jobsADVs["timechange"].Count;
                    openData._data._list[jobsIndex]._args = new Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppStringArray(jobsNum);
                    openData._data._list[jobsIndex]._args[0] = "PC_Job";
                    openData._data._list[jobsIndex]._args[1] = "0,職なし";
                    openData._data._list[jobsIndex]._args[2] = "1,ビーチ監視員";
                    openData._data._list[jobsIndex]._args[3] = "2,カフェ店員";
                    openData._data._list[jobsIndex]._args[4] = "3,巫女・男巫";

                    scenarioLenght = 0;
                    foreach (var scenario in jobsADVs["timechange"])
                    {
                        scenarioLenght += scenario.ScenarioParams.Count;
                    }
                    if (scenarioLenght == 0)
                    {
                        MapLoaderPlugin.Log.LogInfo($"ERROR!: No new scenarios");
                        return;
                    }

                    scenarioData = new Il2CppReferenceArray<ScenarioCommand>(scenarioLenght);

                    index = 0;
                    newJobsIndex = 5;
                    foreach (var jobAdv in jobsADVs["timechange"])
                    {
                        openData._data._list[jobsIndex]._args[newJobsIndex] = jobAdv.JobNameID;

                        foreach (var scenarios in jobAdv.ScenarioParams)
                        {
                            scenarioData[index] = new ScenarioCommand();
                            scenarioData[index]._version = scenarios.Version;
                            scenarioData[index]._multi = scenarios.Multi;
                            scenarioData[index]._command = (Command)Enum.Parse(typeof(Command), scenarios.Command);
                            scenarioData[index]._args = new Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppStringArray(scenarios.Args.Length);
                            for (int i = 0; i < scenarios.Args.Length; i++)
                            {
                                scenarioData[index]._args[i] = scenarios.Args[i];
                            }
                            scenarioData[index].Hash = scenarioData[index].GetHashCode();
                            index++;
                        }
                        newJobsIndex++;
                    }

                    maxLenght = openData._data._list.Count + scenarioLenght;
                    tempScenario = new Il2CppReferenceArray<ScenarioCommand>(maxLenght);

                    pos = 0;
                    for (int i = 0; i < (tempScenario.Count - 2); i++)
                    {
                        if (i > startAdding)
                        {
                            tempScenario[i] = new ScenarioCommand();
                            tempScenario[i]._version = scenarioData[pos]._version;
                            tempScenario[i]._multi = scenarioData[pos]._multi;
                            tempScenario[i]._command = scenarioData[pos]._command;
                            tempScenario[i]._args = scenarioData[pos]._args;
                            tempScenario[i].Hash = tempScenario[i].GetHashCode();
                            pos++;
                        }
                        else
                        {
                            tempScenario[i] = new ScenarioCommand();
                            tempScenario[i]._version = openData._data._list[i]._version;
                            tempScenario[i]._multi = openData._data._list[i]._multi;
                            tempScenario[i]._command = openData._data._list[i]._command;
                            tempScenario[i]._args = openData._data._list[i]._args;
                            tempScenario[i].Hash = tempScenario[i].GetHashCode();
                        }
                    }

                    tempScenario[maxLenght - 2] = new ScenarioCommand();
                    tempScenario[maxLenght - 2]._version = 0;
                    tempScenario[maxLenght - 2]._multi = false;
                    tempScenario[maxLenght - 2]._command = Command.Tag;
                    tempScenario[maxLenght - 2]._args = new Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppStringArray(1);
                    tempScenario[maxLenght - 2]._args[0] = "END";

                    tempScenario[maxLenght - 1] = new ScenarioCommand();
                    tempScenario[maxLenght - 1]._version = 0;
                    tempScenario[maxLenght - 1]._multi = false;
                    tempScenario[maxLenght - 1]._command = Command.Close;

                    openData._data._list = tempScenario;
                    break;
            }
        }      
        public static void ThirdPOV()
        {

        }
    }
}
