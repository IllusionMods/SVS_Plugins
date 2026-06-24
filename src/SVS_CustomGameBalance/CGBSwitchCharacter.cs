using System;
using ADV;
using BepInEx.Logging;
using Manager;
using SV;
using SV.CharaSelectScene;
using SV.CoordeSelectScene;
using SV.CorrelationDiagramScene;
using SV.H;
using SV.MapSelectScene;
using SV.MyRoomScene;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace SVS_CustomGameBalance
{
    internal class CGBSwitchCharacter
    {
        public static void CreateSwitchPCCharacterButton(SimulationScene simInstance)
        {
            var charaInfo = simInstance.transform.Find("Canvas/MainCanvas/GameCanvas/CharaInfo");
            if (charaInfo != null)
            {
                var btnPCSwitch = simInstance.transform.Find("Canvas/MainCanvas/GameCanvas/CharaInfo/btn_Switch");
                if (btnPCSwitch != null) return;
                GameObject PCSwitch = new GameObject("btn_Switch");
                PCSwitch.layer = 5;
                PCSwitch.AddComponent<RectTransform>();
                PCSwitch.AddComponent<CanvasRenderer>();
                PCSwitch.AddComponent<Image>();
                PCSwitch.AddComponent<Button>();

                PCSwitch.transform.SetParent(charaInfo.transform);

                var btnRect = PCSwitch.GetComponent<RectTransform>();
                btnRect.position = new Vector3(490, 1047, 0);
                btnRect.sizeDelta = new Vector2(136, 45);
                btnRect.pivot = new Vector2(0.5f, 0.5f);
                btnRect.anchoredPosition = new Vector3(490f, -33f, 0);
                btnRect.anchoredPosition3D = new Vector3(490f, -33f, 0);
                btnRect.anchorMin = new Vector2(0, 0);
                btnRect.anchorMax = new Vector2(1, 1);
                btnRect.offsetMin = new Vector2(422f, -55.5f);
                btnRect.offsetMax = new Vector2(558f, -10.5f);
                btnRect.localScale = new Vector3(1, 1, 1);//Reset Local scale

                Texture2D btnIcon = CGBLoader.LoadSprite();
                Sprite newSprite = Sprite.Create(btnIcon, new Rect(0.0f, 0.0f, btnIcon.width, btnIcon.height), new Vector2(0f, 1f), 100.0f);
                var btnImage = PCSwitch.GetComponent<Image>();
                btnImage.sprite = newSprite;
                btnImage.name = "btnSwitchSp";
                var btn = PCSwitch.GetComponent<Button>();
                btn.image = btnImage;
                UnityAction act = new Action(() => { SwitchPCCharacter(); });
                btn.onClick.AddListener(act);
                CustomGameBalancePlugin.Log.LogInfo($"Switch PC Button Created");
            }
            else CustomGameBalancePlugin.Log.LogInfo($"Transform CharaInfo not found, failed to create PC Switch Button");
        }
        public static void SwitchPCCharacter()
        {
            if (SimulationManager.Instance == null) return;
            if (GameChara.PlayerAI == null) return;

            if (GameChara.PlayerAI.particleCircles[1].gameObject.active) return;
            if (GameChara.PlayerAI.particleCircles[2].gameObject.active) return;
            if (GameChara.PlayerAI.particleCircles[3].gameObject.active) return;

            var isHScene = ADVManager._instance.IsHScene || (HScene._instance != null && HScene._instance.isActiveAndEnabled);

            var isNotFree = isHScene || ADVManager._instance.IsADV
                || (MyRoom._instance != null && MyRoom._instance.IsOpen())
                || (MapSelect._instance != null && MapSelect._instance.IsOpen())
                || (CharaSelect._instance != null && CharaSelect._instance.IsOpen())
                || (CoordeSelect._instance != null && CoordeSelect._instance.IsOpen())
                || (CorrelationDiagram._instance != null && CorrelationDiagram._instance.IsOpen());
            if (isNotFree) return;

            var np = SimulationManager.Instance.GetCharaWithPlayer();
            if (np != null && np.Count > 0)
            {
                foreach (var c in np)
                {
                    if (c.objCircle.active)
                    {
                        if (c.particleCircles[0].gameObject.active)
                        {
                            //Set NPC values to Active PC
                            GameChara.PlayerAI.BehaviourCtrl.isThinking = true;
                            GameChara.PlayerAI.BehaviourCtrl.isAuto = false;
                            GameChara.PlayerAI.objCircle.active = false;
                            GameChara.Player.charasGameParam.isPC = false;
                            GameChara.PlayerAI.BehaviourCtrl.isPC = false;

                            //Set PC values to NPC
                            c.charaData.charasGameParam.isPC = true;
                            c.BehaviourCtrl.isThinking = false;
                            c.BehaviourCtrl.isPC = true;
                            c.charaData.charasGameParam.baseParameter.NowStamina = GameChara.PlayerAI.charaData.charasGameParam.baseParameter.NowStamina;
                            string name = c.charaData.Name;
                            GameChara.SetPlayer(c.charaData);
                            GameChara.PlayerAI.objCircle.active = true;

                            SV.Sound.Play(SystemSE.sel);
                            return;
                        }

                        SV.Sound.Play(SystemSE.back);
                        CustomGameBalancePlugin.Log.Log(LogLevel.Message, $"Can not switch, NPC is busy!");
                        return;
                    }
                }
            }
            SV.Sound.Play(SystemSE.cancel);
        }
    }
}
