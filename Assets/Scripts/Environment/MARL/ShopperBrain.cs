public interface IShopperBrain
{
    void WhatToDo(CombinedObservations observations,ShopperAgentController shopper);

    void WhereToGo(CombinedObservations observations, ShopperAgentController shopper);

    void GoToDistraction(CombinedObservations observations, ShopperAgentController shopper);
}
