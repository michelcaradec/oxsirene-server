using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace OxSirene.Console
{
    internal static class EstimateDeliveryCmd
    {
        public static async Task ProcessAsync(string uriString, string locationArgument, bool verbose)
        {
            if (string.IsNullOrEmpty(uriString))
            {
                UI.PrintError("Missing argument: URI");
                return;
            }
            Uri uri;
            if (!Uri.TryCreate(uriString, UriKind.Absolute, out uri))
            {
                UI.PrintError("Invalid URI argument: {0}", uriString);
                return;
            }
            if (string.IsNullOrEmpty(API.Configuration.Instance.RequestHeaderFrom))
            {
                UI.PrintFromWarning();
            }

            (API.GeoPoint buyerCoordinates, string buyerAddress) = await GuessBuyerLocationAsync(locationArgument);

            var items = await GetEstimateDeliveryItemsAsync(uri, buyerCoordinates, buyerAddress);

            if (!string.IsNullOrEmpty(buyerAddress))
            {
                UI.PrintInfo($"Buyer estimated address: {buyerAddress}");
            }
            if (buyerCoordinates != null)
            {
                UI.PrintInfo($"Buyer estimated coordinates: {buyerCoordinates}");
                UI.PrintInfo($"  https://www.openstreetmap.org/#map={OSM.ZoomLevel}/{buyerCoordinates.Lat}/{buyerCoordinates.Lon}");
            }

            if (items.Any())
            {
                UI.PrintInfo($"Product: {items.First().ProductName}");
                UI.PrintInfo(string.Empty);

                foreach (var item in items.OrderBy(it => it.EstimateDelivery.CarbonPrint))
                {
                    UI.PrintInfo($"{item.SellerName} ({item.Siren})");
                    UI.PrintInfo($"  {item.SellerAddress}");
                    UI.PrintInfo($"  https://www.openstreetmap.org/#map={OSM.ZoomLevel}/{item.SellerCoordinates.Lat}/{item.SellerCoordinates.Lon}");
                    UI.PrintInfo($"  Great Circle: {item.EstimateDelivery.GreatCircle:.02} Km");
                    UI.PrintInfo($"  Distance: {item.EstimateDelivery.Distance:.02} Km");
                    UI.PrintInfo($"  Carbon Print: {item.EstimateDelivery.CarbonPrint:.02} Kg");
                    UI.PrintInfo(string.Empty);
                }
            }
            else
            {
                UI.PrintWarning("No seller found on French territory");
            }
        }

        private static async Task<(API.GeoPoint coordinates, string address)> GuessBuyerLocationAsync(string locationArgument)
        {
            API.GeoPoint coordinates = null;
            if (!GuessLocationUtils.TryParse(locationArgument, out API.GuessLocationRequest requestLocation))
            {
                coordinates
                    = new API.GeoPoint(
                        double.Parse(API.Configuration.Instance["console:location:longitude"]),
                        double.Parse(API.Configuration.Instance["console:location:latitude"])
                    );
                requestLocation = new API.GuessLocationRequest(coordinates);
            }

            var responseLocation = await API.GuessLocation.RunAsync(requestLocation);
            
            return (responseLocation.Coordinates ?? coordinates, responseLocation.Address);
        }

        private static async Task<IEnumerable<EstimateDeliveryItem>> GetEstimateDeliveryItemsAsync(
            Uri uri,
            API.GeoPoint buyerCoordinates,
            string buyerAddress
        )
        {
            var items = new List<EstimateDeliveryItem>();

            var responseProduct = await API.ScrapProduct.RunAsync(new API.ScrapProductRequest(uri));
            if (responseProduct.IsValid && responseProduct.SellerIDs.Any())
            {
                var responseToken = await API.GetSireneAccessToken.RunAsync(new API.GetSireneAccessTokenRequest());
                if (responseToken.IsValid)
                {
                    foreach (var sellerID in responseProduct.SellerIDs.Distinct())
                    {
                        var responseSeller = await API.ScrapSeller.RunAsync(new API.ScrapSellerRequest(responseProduct.MarketPlaceID, sellerID));
                        if (responseSeller.IsValid && !string.IsNullOrEmpty(responseSeller.Siren))
                        {
                            var responseSirene = await API.QuerySirene.RunAsync(
                                API.QuerySireneRequest.FromSiren(responseToken.Token, responseSeller.Siren)
                            );
                            if (responseSirene.IsValid && responseSirene.Organizations.Count() > 0)
                            {
                                var organization = responseSirene.Organizations.First();
                                var responseBAN = await API.QueryBAN.RunAsync(
                                    new API.QueryBANRequest(organization.Address)
                                );
                                if (responseBAN.IsValid)
                                {
                                    var responseDelivery = await API.EstimateDelivery.RunAsync(
                                        new API.EstimateDeliveryRequest(responseBAN.Coordinates, buyerCoordinates)
                                    );
                                    if (responseDelivery.IsValid)
                                    {
                                        items.Add(
                                            new EstimateDeliveryItem(
                                                responseProduct.ProductName,
                                                responseSeller.Siren,
                                                organization.Name,
                                                responseBAN.Address,
                                                responseBAN.Coordinates,
                                                buyerAddress,
                                                responseDelivery
                                            )
                                        );
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return items;
        }
    }
}
