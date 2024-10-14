namespace SoundBoard

open SoundBoard
open System
open System.Drawing
open System.IO
open System.Media
open System.Windows.Forms
open Microsoft.Extensions.Logging

type MyContext(logger: ILogger<MyContext>, trayIcon: NotifyIcon) =
    inherit ApplicationContext()

    static let mutable playCount = 0

    new(logger: ILogger<MyContext>) =
        let components = new ComponentModel.Container()

        let notifyIcon = new NotifyIcon(components)
        notifyIcon.Icon <- new Icon("Resources/AppIcon.ico")
        notifyIcon.Visible <- true

        let contextMenu = new ContextMenuStrip(components)

        contextMenu.Items.Add("Quitter", null, EventHandler(fun _sender _e -> Application.Exit()))
        |> ignore

        notifyIcon.ContextMenuStrip <- contextMenu

        Keyboard.SetHook(MyContext.HandleKeyPress(Directory.GetFiles("Resources/SoundBites")))

        logger.LogInformation("Ready")

        new MyContext(logger, notifyIcon)

    static member private HandleKeyPress files _key =
        if Random.Shared.Next(10_000) = 0 then
            use player = new SoundPlayer(files[playCount % files.Length])
            player.Play()

            playCount <- playCount + 1

            if playCount = files.Length then
                let desktopPath =
                    Environment.SpecialFolder.DesktopDirectory |> Environment.GetFolderPath

                for i in [ 1..10 ] do
                    File.Copy("Resources/README.txt", $"{desktopPath}/README{i}.txt", true)


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
