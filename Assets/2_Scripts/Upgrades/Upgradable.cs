using System;
using System.Collections.Generic;
using DNExtensions;
using UnityEngine;


public class Upgradable : MonoBehaviour
{
        [Header("Upgrades")]
        [SerializeField, ReadOnly] private int upgradableID;
        [SerializeField] private SOUpgrade[] availableUpgrades;

        
        [Header("References")]
        [SerializeField] private SOGameSettings gameSettings;
        [SerializeField] private Interactable interactable;
        [SerializeField] private InteractableTooltip interactableTooltip;

        private SOUpgrade _nextUpgrade;
        private List<SOUpgrade> _boughtUpgrades;
        
        public event Action OnUpgradesSetup;
        public event Action<SOUpgrade> OnUpgradeBought;
        public event Action<SOUpgrade> OnNextUpgradeChanged;

        private void OnValidate()
        {
                if (upgradableID <= 0)
                {
                        upgradableID = Mathf.Abs(System.Guid.NewGuid().GetHashCode());
                }
        }

        private void Awake()
        {
                SetUpUpgrades();
        }

        private void OnEnable()
        {
                if (GameManager.Instance)
                {
                        GameManager.Instance.OnDayStarted += OnDayStarted;
                        GameManager.Instance.OnDayFinished += OnDayFinished;
                }
        
                if (interactable)
                {
                        interactable.OnInteract += OnInteracted;
                }

        }
        

        private void OnDisable()
        {
                if (GameManager.Instance)
                {
                        GameManager.Instance.OnDayStarted -= OnDayStarted;
                        GameManager.Instance.OnDayFinished -= OnDayFinished;
                }

                if (interactable)
                {
                        interactable.OnInteract -= OnInteracted;
                }

        }
        

        private void OnInteracted(PlayerInteraction obj)
        {
                if (CanBuyUpgrade(_nextUpgrade))
                {
                        BuyUpgrade(_nextUpgrade);
                }
                else
                {
                        // interactableTooltip.PunchSize();
                }
        }
    
        private void OnDayFinished(SODayData dayData)
        {
                interactable?.SetCanInteract(true);
        }

        private void OnDayStarted(SODayData dayData)
        {
                interactable?.SetCanInteract(false);
        }


        
        
        
        
        private void SetUpUpgrades()
        {
                _boughtUpgrades = new List<SOUpgrade>();
                SetNextUpgrade();
                
                OnUpgradesSetup?.Invoke();
        }

        
        private void BuyUpgrade(SOUpgrade upgrade)
        {
                if (!CanBuyUpgrade(upgrade)) return;
                
                _boughtUpgrades.Add(upgrade);
                OnUpgradeBought?.Invoke(upgrade);
        }

        private bool CanBuyUpgrade(SOUpgrade upgrade)
        {
                if (upgrade == null || !GameManager.Instance || _boughtUpgrades.Contains(upgrade)) return false;
        
                return GameManager.Instance.CurrentCurrency >= upgrade.UpgradeCost;
        }
        
        
        
        private void SetNextUpgrade()
        {
                _nextUpgrade = null;
        
                foreach (var upgrade in availableUpgrades)
                {
                        if (!_boughtUpgrades.Contains(upgrade))
                        {
                                _nextUpgrade = upgrade;
                                break;
                        }
                }
        
                if (_nextUpgrade)
                {
                        interactableTooltip?.SetText("Upgrade", _nextUpgrade.UpgradeDescription,_nextUpgrade.UpgradeCost);
                }
                else
                {
                        interactableTooltip?.SetText("", "No more upgrades available", 0);
                }
                
                OnNextUpgradeChanged?.Invoke(_nextUpgrade);
        }
}



