class Program {
    static void Main(FileInfo? configFile) {
        if(configFile == null || !configFile.Exists) {
            Console.WriteLine("Supply a valid path to a server configuration file.");
            Environment.Exit(1);
        }

        var listener = new AsyncSocketListener(Config.Get(configFile));
        listener.Start();
    }
}

