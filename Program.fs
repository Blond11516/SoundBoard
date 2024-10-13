namespace SoundBoard

open SoundBoard
open System
open System.Drawing
open System.Media
open System.Windows.Forms
open Microsoft.Extensions.Logging

type MyContext(logger: ILogger<MyContext>, trayIcon: NotifyIcon) =
    inherit ApplicationContext()

    new(logger: ILogger<MyContext>) =
        let components = new ComponentModel.Container()

        let notifyIcon = new NotifyIcon(components)
        notifyIcon.Icon <- new Icon("Resources/AppIcon.ico")
        notifyIcon.Visible <- true

        let contextMenu = new ContextMenuStrip(components)

        contextMenu.Items.Add("Quitter", null, EventHandler(fun _sender _e -> Application.Exit()))
        |> ignore

        notifyIcon.ContextMenuStrip <- contextMenu

        Keyboard.SetHook(MyContext.HandleKeyPress)

        logger.LogInformation("Ready")

        new MyContext(logger, notifyIcon)

    static member private HandleKeyPress _key =
        use player = new SoundPlayer("Resources/SoundBites/HeyListen.wav")
        player.Play()

    member _.Exit _sender _e = Application.Exit

module Program =
    [<EntryPoint; STAThread>]
    let Main argv =
        // To customize application configuration such as set high DPI settings or default font,
        // see https://aka.ms/applicationconfiguration.
        let logger =
            LoggerFactory
                .Create(fun builder -> builder.AddConsole() |> ignore)
                .CreateLogger<MyContext>()

        Application.Run(new MyContext(logger))
        0
