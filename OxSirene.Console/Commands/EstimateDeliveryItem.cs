using System;

namespace OxSirene.Console
{
    internal class EstimateDeliveryItem
    {
        public string ProductName { get; private set; }
        public string Siren { get; private set; }
        public string SellerName { get; private set; }
        public string SellerAddress { get; private set; }
        public API.GeoPoint SellerCoordinates { get; private set; }
        public string BuyerAddress { get; private set; }
        public API.EstimateDeliveryResponse EstimateDelivery { get; private set; }

        public EstimateDeliveryItem(
            string productName,
            string siren,
            string sellerName,
            string sellerAddress,
            API.GeoPoint sellerCoordinates,
            string buyerAddress,
            API.EstimateDeliveryResponse estimateDelivery
        )
        {
            ProductName = productName;
            Siren = siren;
            SellerName = sellerName;
            SellerAddress = sellerAddress;
            SellerCoordinates = sellerCoordinates;
            BuyerAddress = buyerAddress;
            EstimateDelivery = estimateDelivery;
        }
    }
}