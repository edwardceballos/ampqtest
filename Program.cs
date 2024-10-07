using MassTransit;
using Spectre.Console;

var appTitle = new FigletText("ActiveMQ Test App")
                .Centered()
                .Color(Color.Green);
            AnsiConsole.Write(appTitle);

            // Fetch connection details from environment variables or use default values
            string defaultHost = "ex-aao-all-0-svc-rte-amq.apps.ocp.minvivienda.gov.co";
            string defaultUsername = "vcnhyZM6";
            string defaultPassword = "a0JNynz";
            int defaultPort = 5672;

            string host = Environment.GetEnvironmentVariable("AMQ_HOST") ?? defaultHost;
            string username = Environment.GetEnvironmentVariable("AMQ_USERNAME") ?? defaultUsername;
            string password = Environment.GetEnvironmentVariable("AMQ_PASSWORD") ?? defaultPassword;
            int port = int.TryParse(Environment.GetEnvironmentVariable("AMQ_PORT"), out int envPort) ? envPort : defaultPort;

            AnsiConsole.MarkupLine($"[bold yellow]Starting MassTransit with ActiveMQ...[/]");
            AnsiConsole.MarkupLine($"[bold green]Using Host:[/] {host}");
            AnsiConsole.MarkupLine($"[bold green]Using Port:[/] {port}");
            AnsiConsole.MarkupLine($"[bold green]Using Username:[/] {username}");

            try
            {
                var busControl = Bus.Factory.CreateUsingActiveMq(cfg =>
                {
                    cfg.Host(host, port, h =>
                    {
                        h.Username(username);
                        h.Password(password);
                    });

                    cfg.ReceiveEndpoint("test-queue", e =>
                    {
                        e.UseMessageRetry(r => r.Interval(5, TimeSpan.FromSeconds(2)));

                        e.Handler<TestMessage>(async context =>
                        {
                            try
                            {
                       
                                await Console.Out.WriteLineAsync($"Received: {context.Message.Text}");
                            }
                            catch (Exception ex)
                            {
                                // Log exception for debugging
                                AnsiConsole.MarkupLine($"[bold red]Error in message handling: {ex.Message}[/]");
                                throw; // Re-throw the exception to trigger retry
                            }
                        });
                    });
                });

                // Start the bus
                await busControl.StartAsync();
                try
                {
                    // Send and receive test messages in the same application
                    await SendAndReceiveMessages(busControl);
                    
                    // Keep application running to receive more messages
                    AnsiConsole.MarkupLine("[bold yellow]Press [red]Enter[/] to exit.[/]");
                    Console.ReadLine();
                }
                finally
                {
                    await busControl.StopAsync();
                }
            }
            catch (MassTransitException ex)
            {
                // Handle MassTransit exceptions such as connection issues
                AnsiConsole.MarkupLine($"[bold red]Connection error: {ex.Message}[/]");
            }
            catch (Exception ex)
            {
                // Handle any other unforeseen exceptions
                AnsiConsole.MarkupLine($"[bold red]An error occurred: {ex.Message}[/]");
            }


            async Task SendAndReceiveMessages(IBusControl busControl)
        {
            try
            {
                // Send a test message
                AnsiConsole.MarkupLine("[bold yellow]Sending message to the queue...[/]");
                await busControl.Publish(new TestMessage ( "Hello from ActiveMQ!" ));

                AnsiConsole.MarkupLine("[bold green]Message sent![/]");
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[bold red]Failed to send message: {ex.Message}[/]");
            }
        }

        public class TestMessage
        {
            public TestMessage(string text)
            {
                Text = text;
            }

            public string Text { get; set; }
        }