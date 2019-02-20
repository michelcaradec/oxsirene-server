using System;
using System.Threading.Tasks;

namespace OxSirene.API
{
    public static class EstimateDelivery
    {
        private const double CertuCarCoefficient = 1.4;
        /// <remarks>
        /// http://delartetdubeton.blog.lemonde.fr/2016/02/28/bilan-carbone-faites-le-test/
        /// </remarks>
        private const double CarbonConsumption1km = 0.07;

        public static async Task<EstimateDeliveryResponse> RunAsync(EstimateDeliveryRequest request)
        {
            return await Task.Run(() =>
                {
                    if (request == null)
                    {
                        throw new ArgumentNullException(nameof(request));
                    }
                    if (!request.IsValid)
                    {
                        throw new ArgumentException(nameof(request));
                    }

                    var greatCircle = GreatCircleUtils.GetGreatCircle_v1(request.From, request.To);
                    var distance = greatCircle * CertuCarCoefficient;
                    // "La livraison de votre article contribuera à un empreinte carbone totale de xxx".=> prise en compte d'une livraison multiple.
                    var carbonPrint = distance * CarbonConsumption1km;

                    return new EstimateDeliveryResponse(greatCircle, distance, carbonPrint);
                }
            );
        }
    }
}
