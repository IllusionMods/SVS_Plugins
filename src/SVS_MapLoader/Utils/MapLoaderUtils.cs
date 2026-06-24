using ADV;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Manager;
using System;
using System.Collections.Generic;

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
                default:
                    return -1;
            }
        }

        public static string GetWeekDay()
        {
            int weekDay = Game.saveData.Week;
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
        public static void InsertJobADV(OpenData openData, Dictionary<string, List<MapLoaderParam.JobADV>> jobsADVs)
        {
            if (openData == null || jobsADVs.Count == 0) return;

            Il2CppReferenceArray<ScenarioCommand> scenarioData;
            Il2CppReferenceArray<ScenarioCommand> tempScenario;

            int jobsNum = 5;
            int scenarioLenght;
            int index;
            int newJobsIndex;
            int pos;
            int maxLenght;
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
                    if (!jobsADVs.TryGetValue("s_86", out var s86JobsAdv))
                    {
                        MapLoaderPlugin.Log.LogInfo("ADV s_86 not found");
                        break;
                    }
                    if (s86JobsAdv.Count == 0) break;

                    jobsNum += s86JobsAdv.Count;
                    openData._data._list[jobsIndex]._args = new Il2CppStringArray(jobsNum)
                    {
                        [0] = "PC_Job",
                        [1] = "0,職なし",
                        [2] = "1,ビーチ監視員",
                        [3] = "2,カフェ店員",
                        [4] = "3,巫女・男巫"
                    };

                    scenarioLenght = 0;
                    foreach (var scenario in s86JobsAdv)
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
                    foreach (var jobAdv in s86JobsAdv)
                    {
                        openData._data._list[jobsIndex]._args[newJobsIndex] = jobAdv.JobNameID;

                        foreach (var scenarios in jobAdv.ScenarioParams)
                        {
                            scenarioData[index] = new ScenarioCommand
                            {
                                _version = scenarios.Version,
                                _multi = scenarios.Multi,
                                _command = (Command)Enum.Parse(typeof(Command), scenarios.Command),
                                _args = new Il2CppStringArray(scenarios.Args.Length)
                            };
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
                            tempScenario[i] = new ScenarioCommand
                            {
                                _version = scenarioData[pos]._version,
                                _multi = scenarioData[pos]._multi,
                                _command = scenarioData[pos]._command,
                                _args = scenarioData[pos]._args
                            };
                            tempScenario[i].Hash = tempScenario[i].GetHashCode();
                            pos++;
                        }
                        else
                        {
                            tempScenario[i] = new ScenarioCommand
                            {
                                _version = openData._data._list[i]._version,
                                _multi = openData._data._list[i]._multi,
                                _command = openData._data._list[i]._command,
                                _args = openData._data._list[i]._args
                            };
                            tempScenario[i].Hash = tempScenario[i].GetHashCode();
                        }
                    }

                    tempScenario[maxLenght - 2] = new ScenarioCommand
                    {
                        _version = 0,
                        _multi = false,
                        _command = Command.Tag,
                        _args = new Il2CppStringArray(1)
                        {
                            [0] = "END"
                        }
                    };
                    tempScenario[maxLenght - 2].Hash = tempScenario[maxLenght - 2].GetHashCode();

                    tempScenario[maxLenght - 1] = new ScenarioCommand
                    {
                        _version = 0,
                        _multi = false,
                        _command = Command.Close
                    };
                    //tempScenario[maxLenght - 1]._args = new Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppStringArray(0);
                    tempScenario[maxLenght - 1].Hash = tempScenario[maxLenght - 1].GetHashCode();

                    openData._data._list = tempScenario;
                    break;

                case "a_25_1":
                    if (!jobsADVs.TryGetValue("a_25_1", out var a25JobsAdv))
                    {
                        MapLoaderPlugin.Log.LogInfo("ADV a_25_1 not found");
                        break;
                    }
                    if (a25JobsAdv.Count == 0) break;

                    jobsNum += a25JobsAdv.Count;
                    openData._data._list[jobsIndex]._args = new Il2CppStringArray(jobsNum)
                    {
                        [0] = "PC_Job",
                        [1] = "0,職なし",
                        [2] = "1,ビーチ監視員",
                        [3] = "2,カフェ店員",
                        [4] = "3,巫女・男巫"
                    };

                    scenarioLenght = 0;
                    foreach (var scenario in a25JobsAdv)
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
                    foreach (var jobAdv in a25JobsAdv)
                    {
                        openData._data._list[jobsIndex]._args[newJobsIndex] = jobAdv.JobNameID;

                        foreach (var scenarios in jobAdv.ScenarioParams)
                        {
                            scenarioData[index] = new ScenarioCommand
                            {
                                _version = scenarios.Version,
                                _multi = scenarios.Multi,
                                _command = (Command)Enum.Parse(typeof(Command), scenarios.Command),
                                _args = new Il2CppStringArray(scenarios.Args.Length)
                            };
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
                            tempScenario[i] = new ScenarioCommand
                            {
                                _version = scenarioData[pos]._version,
                                _multi = scenarioData[pos]._multi,
                                _command = scenarioData[pos]._command,
                                _args = scenarioData[pos]._args
                            };
                            tempScenario[i].Hash = tempScenario[i].GetHashCode();
                            pos++;
                        }
                        else
                        {
                            tempScenario[i] = new ScenarioCommand
                            {
                                _version = openData._data._list[i]._version,
                                _multi = openData._data._list[i]._multi,
                                _command = openData._data._list[i]._command,
                                _args = openData._data._list[i]._args
                            };
                            tempScenario[i].Hash = tempScenario[i].GetHashCode();
                        }
                    }

                    tempScenario[maxLenght - 2] = new ScenarioCommand
                    {
                        _version = 0,
                        _multi = false,
                        _command = Command.Tag,
                        _args = new Il2CppStringArray(1)
                        {
                            [0] = "END"
                        }
                    };

                    tempScenario[maxLenght - 1] = new ScenarioCommand
                    {
                        _version = 0,
                        _multi = false,
                        _command = Command.Close
                    };

                    openData._data._list = tempScenario;

                    break;

                case "p_25_1":
                    if (!jobsADVs.TryGetValue("p_25_1", out var p25JobsAdv))
                    {
                        MapLoaderPlugin.Log.LogInfo("ADV p_25_1 not found");
                        break;
                    }
                    if (p25JobsAdv.Count == 0) break;

                    jobsNum += p25JobsAdv.Count;
                    openData._data._list[jobsIndex]._args = new Il2CppStringArray(jobsNum)
                    {
                        [0] = "NPC_Job",
                        [1] = "0,職なし",
                        [2] = "1,ビーチ監視員",
                        [3] = "2,カフェ店員",
                        [4] = "3,巫女・男巫"
                    };

                    scenarioLenght = 0;
                    foreach (var scenario in p25JobsAdv)
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
                    foreach (var jobAdv in p25JobsAdv)
                    {
                        openData._data._list[jobsIndex]._args[newJobsIndex] = jobAdv.JobNameID;

                        foreach (var scenarios in jobAdv.ScenarioParams)
                        {
                            scenarioData[index] = new ScenarioCommand
                            {
                                _version = scenarios.Version,
                                _multi = scenarios.Multi,
                                _command = (Command)Enum.Parse(typeof(Command), scenarios.Command),
                                _args = new Il2CppStringArray(scenarios.Args.Length)
                            };
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
                            tempScenario[i] = new ScenarioCommand
                            {
                                _version = scenarioData[pos]._version,
                                _multi = scenarioData[pos]._multi,
                                _command = scenarioData[pos]._command,
                                _args = scenarioData[pos]._args
                            };
                            tempScenario[i].Hash = tempScenario[i].GetHashCode();
                            pos++;
                        }
                        else
                        {
                            tempScenario[i] = new ScenarioCommand
                            {
                                _version = openData._data._list[i]._version,
                                _multi = openData._data._list[i]._multi,
                                _command = openData._data._list[i]._command,
                                _args = openData._data._list[i]._args
                            };
                            tempScenario[i].Hash = tempScenario[i].GetHashCode();
                        }
                    }

                    tempScenario[maxLenght - 2] = new ScenarioCommand
                    {
                        _version = 0,
                        _multi = false,
                        _command = Command.Tag,
                        _args = new Il2CppStringArray(1)
                        {
                            [0] = "END"
                        }
                    };

                    tempScenario[maxLenght - 1] = new ScenarioCommand
                    {
                        _version = 0,
                        _multi = false,
                        _command = Command.Close
                    };

                    openData._data._list = tempScenario;

                    break;

                case "a_43_1":
                    if (!jobsADVs.TryGetValue("a_43_1", out var a43JobsAdv))
                    {
                        MapLoaderPlugin.Log.LogInfo("ADV a_43_1 not found");
                        break;
                    }
                    if (a43JobsAdv.Count == 0) break;

                    jobsNum += a43JobsAdv.Count;
                    openData._data._list[jobsIndex]._args = new Il2CppStringArray(jobsNum)
                    {
                        [0] = "PC_Job",
                        [1] = "0,職なし",
                        [2] = "1,ビーチ監視員",
                        [3] = "2,カフェ店員",
                        [4] = "3,巫女・男巫"
                    };

                    scenarioLenght = 0;
                    foreach (var scenario in a43JobsAdv)
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
                    foreach (var jobAdv in a43JobsAdv)
                    {
                        openData._data._list[jobsIndex]._args[newJobsIndex] = jobAdv.JobNameID;

                        foreach (var scenarios in jobAdv.ScenarioParams)
                        {
                            scenarioData[index] = new ScenarioCommand
                            {
                                _version = scenarios.Version,
                                _multi = scenarios.Multi,
                                _command = (Command)Enum.Parse(typeof(Command), scenarios.Command),
                                _args = new Il2CppStringArray(scenarios.Args.Length)
                            };
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
                            tempScenario[i] = new ScenarioCommand
                            {
                                _version = scenarioData[pos]._version,
                                _multi = scenarioData[pos]._multi,
                                _command = scenarioData[pos]._command,
                                _args = scenarioData[pos]._args
                            };
                            tempScenario[i].Hash = tempScenario[i].GetHashCode();
                            pos++;
                        }
                        else
                        {
                            tempScenario[i] = new ScenarioCommand
                            {
                                _version = openData._data._list[i]._version,
                                _multi = openData._data._list[i]._multi,
                                _command = openData._data._list[i]._command,
                                _args = openData._data._list[i]._args
                            };
                            tempScenario[i].Hash = tempScenario[i].GetHashCode();
                        }
                    }

                    tempScenario[maxLenght - 2] = new ScenarioCommand
                    {
                        _version = 0,
                        _multi = false,
                        _command = Command.Tag,
                        _args = new Il2CppStringArray(1)
                        {
                            [0] = "END"
                        }
                    };

                    tempScenario[maxLenght - 1] = new ScenarioCommand
                    {
                        _version = 0,
                        _multi = false,
                        _command = Command.Close
                    };

                    openData._data._list = tempScenario;
                    break;
                case "p_43_1":
                    if (!jobsADVs.TryGetValue("p_43_1", out var p43JobsAdv))
                    {
                        MapLoaderPlugin.Log.LogInfo("ADV p_43_1 not found");
                        break;
                    }
                    if (p43JobsAdv.Count == 0) break;

                    jobsNum += p43JobsAdv.Count;
                    openData._data._list[jobsIndex]._args = new Il2CppStringArray(jobsNum)
                    {
                        [0] = "NPC_Job",
                        [1] = "0,職なし",
                        [2] = "1,ビーチ監視員",
                        [3] = "2,カフェ店員",
                        [4] = "3,巫女・男巫"
                    };

                    scenarioLenght = 0;
                    foreach (var scenario in p43JobsAdv)
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
                    foreach (var jobAdv in p43JobsAdv)
                    {
                        openData._data._list[jobsIndex]._args[newJobsIndex] = jobAdv.JobNameID;

                        foreach (var scenarios in jobAdv.ScenarioParams)
                        {
                            scenarioData[index] = new ScenarioCommand
                            {
                                _version = scenarios.Version,
                                _multi = scenarios.Multi,
                                _command = (Command)Enum.Parse(typeof(Command), scenarios.Command),
                                _args = new Il2CppStringArray(scenarios.Args.Length)
                            };
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
                            tempScenario[i] = new ScenarioCommand
                            {
                                _version = scenarioData[pos]._version,
                                _multi = scenarioData[pos]._multi,
                                _command = scenarioData[pos]._command,
                                _args = scenarioData[pos]._args
                            };
                            tempScenario[i].Hash = tempScenario[i].GetHashCode();
                            pos++;
                        }
                        else
                        {
                            tempScenario[i] = new ScenarioCommand
                            {
                                _version = openData._data._list[i]._version,
                                _multi = openData._data._list[i]._multi,
                                _command = openData._data._list[i]._command,
                                _args = openData._data._list[i]._args
                            };
                            tempScenario[i].Hash = tempScenario[i].GetHashCode();
                        }
                    }

                    tempScenario[maxLenght - 2] = new ScenarioCommand
                    {
                        _version = 0,
                        _multi = false,
                        _command = Command.Tag,
                        _args = new Il2CppStringArray(1)
                        {
                            [0] = "END"
                        }
                    };

                    tempScenario[maxLenght - 1] = new ScenarioCommand
                    {
                        _version = 0,
                        _multi = false,
                        _command = Command.Close
                    };

                    openData._data._list = tempScenario;
                    break;
                case "timechange":
                    if (!jobsADVs.TryGetValue("timechange", out var timechangeJobsAdv))
                    {
                        MapLoaderPlugin.Log.LogInfo("ADV timechange not found");
                        break;
                    }
                    if (timechangeJobsAdv.Count == 0) break;

                    jobsNum += timechangeJobsAdv.Count;
                    openData._data._list[jobsIndex]._args = new Il2CppStringArray(jobsNum)
                    {
                        [0] = "PC_Job",
                        [1] = "0,職なし",
                        [2] = "1,ビーチ監視員",
                        [3] = "2,カフェ店員",
                        [4] = "3,巫女・男巫"
                    };

                    scenarioLenght = 0;
                    foreach (var scenario in timechangeJobsAdv)
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
                    foreach (var jobAdv in timechangeJobsAdv)
                    {
                        openData._data._list[jobsIndex]._args[newJobsIndex] = jobAdv.JobNameID;

                        foreach (var scenarios in jobAdv.ScenarioParams)
                        {
                            scenarioData[index] = new ScenarioCommand
                            {
                                _version = scenarios.Version,
                                _multi = scenarios.Multi,
                                _command = (Command)Enum.Parse(typeof(Command), scenarios.Command),
                                _args = new Il2CppStringArray(scenarios.Args.Length)
                            };
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
                            tempScenario[i] = new ScenarioCommand
                            {
                                _version = scenarioData[pos]._version,
                                _multi = scenarioData[pos]._multi,
                                _command = scenarioData[pos]._command,
                                _args = scenarioData[pos]._args
                            };
                            tempScenario[i].Hash = tempScenario[i].GetHashCode();
                            pos++;
                        }
                        else
                        {
                            tempScenario[i] = new ScenarioCommand
                            {
                                _version = openData._data._list[i]._version,
                                _multi = openData._data._list[i]._multi,
                                _command = openData._data._list[i]._command,
                                _args = openData._data._list[i]._args
                            };
                            tempScenario[i].Hash = tempScenario[i].GetHashCode();
                        }
                    }

                    tempScenario[maxLenght - 2] = new ScenarioCommand
                    {
                        _version = 0,
                        _multi = false,
                        _command = Command.Tag,
                        _args = new Il2CppStringArray(1)
                        {
                            [0] = "END"
                        }
                    };

                    tempScenario[maxLenght - 1] = new ScenarioCommand
                    {
                        _version = 0,
                        _multi = false,
                        _command = Command.Close
                    };

                    openData._data._list = tempScenario;
                    break;
            }
        }
        //public static void ThirdPOV()
        //{
        //
        //}
    }
}
