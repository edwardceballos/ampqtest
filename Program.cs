              using MassTransit;
              using Spectre.Console;

              var appTitle = new FigletText("ActiveMQ Test App")
                .Centered()
                .Color(Color.Green);
            AnsiConsole.Write(appTitle);

            AnsiConsole.MarkupLine("[bold yellow]Enter ActiveMQ Connection Details (press Enter to use default values):[/]");

            string defaultHost = "ex-aao-hdls-svc.amq.svc.cluster.local";
            string defaultUsername = "vcnhyZM6";
            string defaultPassword = "a0JNynz";
            int defaultPort = 61616;

            var host = AnsiConsole.Ask($"Host: [gray]({defaultHost})[/]", defaultHost);

            var username = AnsiConsole.Ask($"Username: [gray]({defaultUsername})[/]", defaultUsername);

            var password = AnsiConsole.Ask($"Password: [gray]({defaultPassword})[/]", defaultPassword);

            var port = AnsiConsole.Ask($"Port: [gray]({defaultPort})[/]", defaultPort);

            AnsiConsole.MarkupLine("[bold yellow]Starting MassTransit with ActiveMQ...[/]");

            var busControl = Bus.Factory.CreateUsingActiveMq(cfg =>
            {
                cfg.Host(host, port, h =>
                {
                    h.Username(username);
                    h.Password(password);
                });

                cfg.ReceiveEndpoint("test-queue", e =>
                {
                    e.Handler<TestMessage>(context =>
                    {
                        return Console.Out.WriteLineAsync($"Received: {context.Message.Text}");
                    });
                });
            });

            await busControl.StartAsync();
            try
            {
                AnsiConsole.MarkupLine("[bold yellow]Sending message to the queue...[/]");
                await busControl.Publish(new TestMessage (  "Hello from ActiveMQ!" ));

                AnsiConsole.MarkupLine("[bold green]Message sent![/]");
            
                await Task.Delay(500);

                AnsiConsole.MarkupLine("[bold yellow]Message received![/]");
            }
            finally
            {
                await busControl.StopAsync();
            }

            public class TestMessage
        {
            public TestMessage(string text)
            {
                Text = text;
            }


            public string Text { get; set; }
        }
