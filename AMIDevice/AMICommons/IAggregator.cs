using System.ServiceModel;

namespace AMICommons
{
    /// <summary>
    /// WCF interfejs namenjen za slanje poruka od uredjaja prema agregatoru
    /// </summary>
    [ServiceContract]
    public interface IAggregator
    { 
        /// <summary>
        /// Metoda za inicijalno povezivanje uredjaja sa agregatorom. Pozvati pre SendMeasurement.
        /// </summary>
        /// <param name="measurement">Prvo merenje uredjaja</param>
        /// <returns>True u slucaju uspesne konekcije, false u slucaju neuspesne, sto znaci reinicijalizaciju uredjaja i ponovno pokusavanje povezivanja.</returns>
        [OperationContract]
        bool Connect(AMIMeasurement measurement);

        /// <summary>
        /// Metoda za slanje merenja posle konekcije preko Connect.
        /// </summary>
        /// <param name="measurement">Merenje uredjaja</param>
        /// <returns>True u slucaju uspesne konekcije, false u slucaju neuspesne.</returns>
        [OperationContract]
        bool SendMeasurement(AMIMeasurement measurement);
    }
}
