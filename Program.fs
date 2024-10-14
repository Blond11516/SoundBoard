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

    static let assembly = typeof<MyContext>.Assembly
    static let resourcesPrefix = "SoundBoard.Resources."

    static let getResource name =
        assembly.GetManifestResourceStream($"{resourcesPrefix}{name}")

    static let resourceNames = assembly.GetManifestResourceNames()

    static let soundBiteNames =
        resourceNames
        |> Array.filter (fun it -> it.StartsWith($"{resourcesPrefix}SoundBites."))
        |> Array.map (fun it -> it.Replace(resourcesPrefix, ""))
        |> Array.sortBy (fun it -> it.Substring(0, 1))

    new(logger: ILogger<MyContext>) =
        let components = new ComponentModel.Container()

        let notifyIcon = new NotifyIcon(components)
        let resource = getResource ("AppIcon.ico")
        notifyIcon.Icon <- new Icon(resource)
        notifyIcon.Visible <- true

        let contextMenu = new ContextMenuStrip(components)

        contextMenu.Items.Add("Quitter", null, EventHandler(fun _sender _e -> Application.Exit()))
        |> ignore

        notifyIcon.ContextMenuStrip <- contextMenu

        Keyboard.SetHook(MyContext.HandleKeyPress)

        logger.LogInformation("Ready")

        new MyContext(logger, notifyIcon)

    static member private HandleKeyPress _key =
        if Random.Shared.Next(1) = 0 then
            let currentSoundBiteName = soundBiteNames[playCount % soundBiteNames.Length]
            use player = new SoundPlayer(getResource (currentSoundBiteName))
            player.Play()

            playCount <- playCount + 1

            if playCount = soundBiteNames.Length then
                let desktopPath =
                    Environment.SpecialFolder.DesktopDirectory |> Environment.GetFolderPath

                use reader = new StreamReader(getResource "README.txt")
                let fileContent = reader.ReadToEnd()

                for i in [ 1..10 ] do
                    File.WriteAllText($"{desktopPath}/README{i}.txt", fileContent)


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
