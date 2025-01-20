using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using System.Security.Claims;
using System.Text.Json.Serialization;
using AutomationTestingProgram.Core;

namespace AutomationTestingProgram.Modules.TestRunnerModule
{
    /// <summary>
    /// Request to retrieve keychain accounts
    /// </summary>
    public class KeyChainRetrievalRequest : NonCancellableClientRequest, IClientRequest
    {
        [JsonIgnore]
        protected override ICustomLogger Logger { get; }

        /// <summary>
        /// List of all accounts found with this request
        /// </summary>
        public IList<object> Accounts { get; }


        /// <summary>
        /// Initializes a new instance of the <see cref="KeyChainRetrievalRequest"/> class.
        /// </summary>
        public KeyChainRetrievalRequest(ICustomLoggerProvider provider, ClaimsPrincipal User)
            :base(User)
        {
            Logger = provider.CreateLogger<KeyChainRetrievalRequest>(FolderPath);

            Accounts = new List<object>();
        }

        /// <summary>
        /// Validate the <see cref="KeyChainRetrievalRequest"/>.
        /// View inner documentation on specifics.  
        /// </summary>
        protected override void Validate()
        {
            /*
             * VALIDATION:
             * - User has permission to access application
             * - User has permission to access application section (requets sent from sections in the application)
             */

            this.SetStatus(State.Validating, $"Validating KeyChainRetrieval Request (ID: {ID})");

            // Validate permission to access application
            LogInfo($"Validating User Permissions - Team");

            /*
             * Later implementation: Validates what emails the user has access to.
             * Only those emails are returned.
             */
        }

        /// <summary>
        /// Execute the <see cref="KeyChainRetrievalRequest"/>.
        /// View inner documentation on specifics.  
        /// </summary>
        protected override async Task Execute()
        {
            try
            {
                this.SetStatus(State.Processing, $"Processing KeyChainRetrieval Request (ID: {ID})");
                await IOManager.TryAquireSlotAsync();


                string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "KeychainAccounts2023.xls");

                if (!File.Exists(filePath))
                {
                    Console.WriteLine("KeychainAccounts2023.xls FILE NOT FOUND. INVESTIGATE!");
                    throw new Exception("KeychainFile not found");
                }

                using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    IWorkbook workbook = new HSSFWorkbook(fs);
                    ISheet sheet = workbook.GetSheetAt(0);

                    for (int rowIdx = 1; rowIdx <= sheet.LastRowNum; rowIdx++)
                    {
                        IRow row = sheet.GetRow(rowIdx);
                        if (row != null)
                        {
                            Accounts.Add(new
                            {
                                email = row.GetCell(14)?.StringCellValue ?? string.Empty,
                                role = row.GetCell(18)?.StringCellValue ?? string.Empty,
                                organization = row.GetCell(19)?.StringCellValue ?? string.Empty
                            });
                        }
                    }
                }


                SetStatus(State.Completed, $"KeyChainRetrieval Request (ID: {ID}) completed successfully");
            }
            finally
            {
                IOManager.ReleaseSlot();
            }
        }
    }
}
