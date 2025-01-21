using System.Security.Claims;
using System.Text.Json.Serialization;
using AutomationTestingProgram.Core;

namespace AutomationTestingProgram.Modules.TestRunnerModule
{
    /// <summary>
    /// Request to validate a test file.
    /// </summary>
    public class ValidationRequest : CancellableClientRequest, IClientRequest
    {
        [JsonIgnore]
        protected override ICustomLogger Logger { get; }

        /// <summary>
        /// The file provided with the request
        /// </summary>
        public IFormFile File { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationRequest"/> class.
        /// Instance is associated with a file.
        /// </summary>
        /// <param name="File">The file to be validated in the request.</param>
        public ValidationRequest(ICustomLoggerProvider provider, ClaimsPrincipal User, IFormFile File)
            :base(User)
        {
            this.Logger = provider.CreateLogger<ValidationRequest>(FolderPath);

            this.File = File;
        }

        /// <summary>
        /// Validate the <see cref="ValidationRequest"/>.
        /// View inner documentation on specifics.  
        /// </summary>
        protected override void Validate()
        {
            /*
             * VALIDATION:
             * - User has permission to access application
             * - User has permission to access application team/group
             */

            this.SetStatus(State.Validating, $"Validating Process Request (ID: {ID})");

            // Validate permission to access team
            LogInfo($"Validating User Permissions - Team");
        }

        /// <summary>
        /// Execute the <see cref="ValidationRequest"/>.
        /// View inner documentation on specifics.  
        /// </summary>
        protected override async Task Execute()
        {
            this.SetStatus(State.Processing, $"Processing Validation Request (ID: {ID})");

            IsCancellationRequested();

            /*for (int i = 0; i <= 5; i++)
            {
                await Task.Delay(20000, CancelToken);
                Logger.LogInformation($"{i}");
            }*/

            LockManager<string> manager = new LockManager<string>(3);

            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.Cancel(); // Immediately cancel the token
            CancellationToken token = cancellationTokenSource.Token;

            var task1 = Task.WhenAll( // A
                Task.Run(async () =>
                {
                    await Task.Delay(1000);

                    var innerTask1 = Task.Run(async () => { 
                        try
                        {
                            await manager.AquireLockAsync("A", token);
                            LogInfo($"{DateTime.Now:HH:mm:ss.fff}  A - 1 completed.");
                            await Task.Delay(5000);
                            manager.ReleaseLock("A");
                        }
                        catch
                        {
                            LogError($"{DateTime.Now:HH:mm:ss.fff}  A - 1 cancelled.");
                        }
                        
                        
                    });
                    var innerTask2 = Task.Run(async () => { await Task.Delay(20); await manager.AquireLockAsync("A"); LogInfo($"{DateTime.Now:HH:mm:ss.fff}  A - 2 completed."); await Task.Delay(5000); manager.ReleaseLock("A"); });
                    // var innerTask3 = Task.Run(async () => { await Task.Delay(40); await manager.AquireLockAsync("A"); Console.WriteLine($"{DateTime.Now:HH:mm:ss.fff}  A - 3 completed."); await Task.Delay(1000); manager.ReleaseLock("A"); });
                    await Task.WhenAll(innerTask1, innerTask2);
                })
            );

            var task2 = Task.WhenAll( // B
                Task.Run(async () =>
                {
                    await Task.Delay(1050);

                    var innerTask1 = Task.Run(async () => { await manager.AquireLockAsync("B"); LogInfo($"{DateTime.Now:HH:mm:ss.fff}  B - 1 completed."); await Task.Delay(5000); manager.ReleaseLock("B"); });
                    var innerTask2 = Task.Run(async () => { await Task.Delay(20); await manager.AquireLockAsync("B"); LogInfo($"{DateTime.Now:HH:mm:ss.fff}  B - 2 completed."); await Task.Delay(5000); manager.ReleaseLock("B"); });
                    var innerTask3 = Task.Run(async () => { await Task.Delay(40); await manager.AquireLockAsync("B"); LogInfo($"{DateTime.Now:HH:mm:ss.fff}  B - 3 completed."); await Task.Delay(5000); manager.ReleaseLock("B"); });
                    var innerTask4 = Task.Run(async () => { 
                        try
                        {
                            await Task.Delay(60);
                            await manager.AquireLockAsync("B", token);
                            LogInfo($"{DateTime.Now:HH:mm:ss.fff}  B - 4 completed.");
                            await Task.Delay(5000);
                            manager.ReleaseLock("B");
                        }
                        catch
                        {
                            LogError($"{DateTime.Now:HH:mm:ss.fff}  B - 4 cancelled.");
                        }
                        
                    });
                    var innerTask5 = Task.Run(async () => { await Task.Delay(80); await manager.AquireLockAsync("B"); LogInfo($"{DateTime.Now:HH:mm:ss.fff}  B - 5 completed."); await Task.Delay(5000); manager.ReleaseLock("B"); });
                    var innerTask6 = Task.Run(async () => { await Task.Delay(100); await manager.AquireLockAsync("B"); LogInfo($"{DateTime.Now:HH:mm:ss.fff}  B - 6 completed."); await Task.Delay(5000); manager.ReleaseLock("B"); });
                    await Task.WhenAll(innerTask1, innerTask2, innerTask3, innerTask4, innerTask5, innerTask6);
                })
            );

            var task3 = Task.WhenAll( // C
                Task.Run(async () =>
                {
                    await Task.Delay(1100);

                    var innerTask1 = Task.Run(async () => { await manager.AquireLockAsync("C"); LogInfo($"{DateTime.Now:HH:mm:ss.fff}  C - 1 completed."); await Task.Delay(5000); manager.ReleaseLock("C"); });
                    var innerTask2 = Task.Run(async () => { await Task.Delay(20); await manager.AquireLockAsync("C"); LogInfo($"{DateTime.Now:HH:mm:ss.fff}  C - 2 completed."); await Task.Delay(5000); manager.ReleaseLock("C"); });
                    var innerTask3 = Task.Run(async () => { await Task.Delay(40); await manager.AquireLockAsync("C"); LogInfo($"{DateTime.Now:HH:mm:ss.fff}  C - 3 completed."); await Task.Delay(5000); manager.ReleaseLock("C"); });
                    await Task.WhenAll(innerTask1, innerTask2, innerTask3);
                })
            );

            var task4 = Task.WhenAll( // D
                Task.Run(async () =>
                {
                    await Task.Delay(1150);

                    var innerTask1 = Task.Run(async () => {
                        try
                        {
                            await manager.AquireLockAsync("D", token);
                            LogInfo($"{DateTime.Now:HH:mm:ss.fff}  D - 1 completed.");
                            await Task.Delay(5000);
                            manager.ReleaseLock("D");
                        }
                        catch
                        {
                            LogError($"{DateTime.Now:HH:mm:ss.fff}  D - 1 cancelled.");
                        }
                        
                    });
                    var innerTask2 = Task.Run(async () => { 
                        try
                        {
                            await Task.Delay(20);
                            await manager.AquireLockAsync("D", token);
                            LogInfo($"{DateTime.Now:HH:mm:ss.fff}  D - 2 completed.");
                            await Task.Delay(5000);
                            manager.ReleaseLock("D");
                        }
                        catch
                        {
                            LogError($"{DateTime.Now:HH:mm:ss.fff}  D - 2 cancelled.");
                        }
                        
                    });
                    var innerTask3 = Task.Run(async () => { 
                        try
                        {
                            await Task.Delay(40);
                            await manager.AquireLockAsync("D", token);
                            LogInfo($"{DateTime.Now:HH:mm:ss.fff}  D - 3 completed.");
                            await Task.Delay(5000);
                            manager.ReleaseLock("D");
                        }
                        catch
                        {
                            LogError($"{DateTime.Now:HH:mm:ss.fff}  D - 3 cancelled.");
                        }
                    });
                    await Task.WhenAll(innerTask1, innerTask2, innerTask3);
                })
            );

            var task5 = Task.WhenAll( // E
                Task.Run(async () =>
                {
                    await Task.Delay(1155);

                    var innerTask1 = Task.Run(async () => { await manager.AquireLockAsync("E"); LogInfo($"{DateTime.Now:HH:mm:ss.fff}  E - 1 completed."); await Task.Delay(5000); manager.ReleaseLock("E"); });
                    var innerTask2 = Task.Run(async () => { await Task.Delay(20); await manager.AquireLockAsync("E"); LogInfo($"{DateTime.Now:HH:mm:ss.fff}  E - 2 completed."); await Task.Delay(5000); manager.ReleaseLock("E"); });
                    var innerTask3 = Task.Run(async () => { await Task.Delay(40); await manager.AquireLockAsync("E"); LogInfo($"{DateTime.Now:HH:mm:ss.fff}  E - 3 completed."); await Task.Delay(5000); manager.ReleaseLock("E"); });
                    await Task.WhenAll(innerTask1, innerTask2, innerTask3);
                })
            );

            var task6 = Task.WhenAll( // F
                Task.Run(async () =>
                {
                    await Task.Delay(2000);

                    var innerTask1 = Task.Run(async () => { await manager.AquireLockAsync("F"); LogInfo($"{DateTime.Now:HH:mm:ss.fff}  F - 1 completed."); await Task.Delay(5000); manager.ReleaseLock("F"); });
                    var innerTask2 = Task.Run(async () => { await Task.Delay(20); await manager.AquireLockAsync("F"); LogInfo($"{DateTime.Now:HH:mm:ss.fff}  F - 2 completed."); await Task.Delay(5000); manager.ReleaseLock("F"); });
                    var innerTask3 = Task.Run(async () => { await Task.Delay(40); await manager.AquireLockAsync("F"); LogInfo($"{DateTime.Now:HH:mm:ss.fff}  F - 3 completed."); await Task.Delay(5000); manager.ReleaseLock("F"); });
                    await Task.WhenAll(innerTask1, innerTask2, innerTask3);
                })
            );

            // Wait for all outer tasks to complete
            await Task.WhenAll(task1, task2, task3, task4, task5, task6);



            this.SetStatus(State.Completed, $"Validation Request (ID: {ID}) completed successfully");
        }
    }
}
