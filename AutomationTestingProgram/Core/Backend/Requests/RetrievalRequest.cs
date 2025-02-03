using System.Security.Claims;
using System.Text.Json.Serialization;

namespace AutomationTestingProgram.Core
{
    /// <summary>
    /// Request to retrieve active requests (with various filters)
    /// </summary>
    public class RetrievalRequest : NonCancellableClientRequest, IClientRequest
    {
        [JsonIgnore]
        protected override ICustomLogger Logger { get; }

        /// <summary>
        /// The type of filter provided with the request
        /// </summary>
        public FilterType FilterType { get; }

        /// <summary>
        /// The value of the filter provided with the request
        /// </summary>
        public string FilterValue { get; }

        /// <summary>
        /// List of retrieved requests, based on filter
        /// </summary>
        public IList<IClientRequest> RetrievedRequests { get; private set; }

        [JsonIgnore]
        private RequestHandler _requestHandler { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RetrievalRequest"/> class.
        /// </summary>
        public RetrievalRequest(ICustomLoggerProvider provider, RequestHandler requestHandler, ClaimsPrincipal User, RetrievalRequestModel model)
            : base(User)
        {
            Logger = provider.CreateLogger<RetrievalRequest>(FolderPath);

            if (Enum.TryParse(model.FilterType, out FilterType parsedType))
            {
                FilterType = parsedType;
            }
            else
            {
                throw new ArgumentException($"Invalid filter type: {model.FilterType}");
            }

            FilterValue = model.FilterValue;
            RetrievedRequests = new List<IClientRequest>();

            _requestHandler = requestHandler;
        }


        /// <summary>
        /// Validate the <see cref="RetrievalRequest"/>.
        /// View inner documentation on specifics.  
        /// </summary>
        protected override void Validate()
        {
            /*
             * VALIDATION:
             * - User has permission to access application
             * - User has permission to access application section (requets sent from sections in the application)
             */

            SetStatus(State.Validating, $"Validating Retrieval Request (ID: {ID})");

            // Validate permission to access application
            LogInfo($"Validating User Permissions - Application");
        }

        /// <summary>
        /// Execute the <see cref="RetrievalRequest"/>.
        /// View inner documentation on specifics.  
        /// </summary>
        protected override Task Execute()
        {
            /* Filters all active requests.
             * Returns all results the user has permission to view.
             * 
             * Permissions:
             * - SuperUser: All requests
             * - Admin: All requests within a team
             * - User: Own requests 
             * 
             */

            SetStatus(State.Processing, $"Processing Retrieval Request (ID: {ID})");

            switch (FilterType)
            {
                case FilterType.None:
                    RetrievedRequests = _requestHandler.RetrieveRequests();
                    break;
                case FilterType.ID:
                    RetrievedRequests.Add(_requestHandler.RetrieveRequest(FilterValue));
                    break;
                case FilterType.Type:
                    RetrievedRequests = _requestHandler.RetrieveRequests(FilterValue);
                    break;
            }

            SetStatus(State.Completed, $"Retrieval Request (ID: {ID}) completed successfully.");

            return Task.CompletedTask;
        }
    }

    /// <summary>
    /// Enum used to desctibe the filter type for Retrieval Requests
    /// </summary>
    public enum FilterType
    {
        /// <summary>
        /// No filter applied (retrieve all possible requests)
        /// </summary>
        None,

        /// <summary>
        /// Looking for a specific request by ID
        /// </summary>
        ID,

        /// <summary>
        /// Looking for a specific request by their Type.
        /// Types are request specific, or abstract classes.
        /// </summary>
        Type
    }
}
