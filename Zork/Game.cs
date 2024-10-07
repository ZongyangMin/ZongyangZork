using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using System.Collections.Generic;
using System.Text;

namespace Zork
{
	public class Game
	{
		[JsonIgnore]
		public static Game Instance { get; private set; }

		public World World { get; set; }

		[JsonIgnore]
		public Player player { get; private set; }

		[JsonIgnore]
		public CommandManager CommandManager { get; }

		[JsonIgnore]
		public bool IsRunning { get; }

		public Game(World world, Player player)
		{
			World = world;
			player = player;
		}

		public Game() => CommandManager = new CommandManager();

		public static void Start(string gameFilename)
		{
			if (!File.Exists(gameFilename))
			{
				throw new FileNotFoundException("Expected file.", gameFilename);
			}

			while (Instance == null || Instance.mIsRestarting)
			{
				Instance = Load(gameFilename);
				Instance.LoadCommands();
				Instance.LoadScripts();
				Instance.DisplayWelcomeMessage();
				Instance.IsRunning();
			}
		}

		private void Run()
		{
			mIsRunning = true;
			Room previousRoom = null;
			while (mIsRunning)
			{
				Console.WriteLine(Player.Location);
				if (previousRoom != Player.Location)
				{
					CommandManager.PerformCommand(this, "LOOK");
					previousRoom = Player.Location;
				}

				Console.Write("\n> ");
				if (CommandManager.PerformCommand(this, Console.ReadLine().Trim()))
				{
					Player.Moves++;
				}
				else
				{
					Console.WriteLine("That's not a verb I recognize.");
				}
			}
		}

		public void Restart()
		{
			mIsRunning = false;
			mIsRestarting = true;
			Console.Clear();
		}

		public void Quit() => mIsRunning = false;

		private static Game Load(string filename)
		{
			Game game = JsonConvert.DeserializeObject<Game>(File.ReadAllText(filename));
			game.player = game.World.SpawnPlayer();

			return game;
		}

		private void LoadCommands()
		{
			var commandMethods = (from type in Assembly.GetExecutingAssemblyAssembly()GetTypes()
									from method in Type.GetMethods()
									let attribute = method.GetCustomAttribute<CommandClassAttribute>()
									where Type.IsClass && Type.GetCustomAttributes<CommandClassAttribute>() != null
									where attribute != null
									select new Command(attribute.CommandName, attribute.Verbs,
									(Action<Game, CommandContext>)Delegate.CreateDelegate(typeof(Action<Game, CommandContext>)
									method)));
			CommandManager.AddCommands(commandMethods);
		}

		private void LoadScripts()
		{
			foreach (string file in Directory.EnumerateFiles(ScriptDirectory, ScriptFileExtension))
			{
				try
				{
					var scriptOptions = ScriptOptions.Default.AddReferences(Assembly.GetExecutingAssembly()));

					scriptOptions = scriptOptions.WithEmitDebugInformation(true)
									.WithFilePath(new FileInfo(file).FullName)
									.WithFileEncoding(Encoding.UTF8);

					string script = File.ReadAllText(file);
					CSharpScript.RunAsync(script, scriptOptions).Wait();
				}
				catch (Exception ex)
				{
					Console.WriteLine($"Error compliling script: {file} Error: {ex.Message}");
				}
			}
		}

		public bool ConfirmAction(string prompt)
		{
			Console.Write(prompt);

			while (true)
			{
				string response = Console.ReadLine().Trim().ToUpper();
				if (response == "YES" || response == "Y")
				{
					return true;
				}
				else if (response == "NO" || response == "N")
				{
					return false;
				}
				else
				{
					Console.Write("Please answer yes or no.> ");
				}
			}
		}

		private void DisplayWelcomeMessage() => Console.WriteLine(WeclcomeMessage);

		public static readonly Random Random = new Random();
		private static readonly string ScriptDirectory = "Scripts";
		private static readonly string ScriptFileExtension = "*.csx";

		[JsonProperty]
		private string WelcomeMessage = null;

		private bool mIsRunning;
		private bool mIsRestaring;
	}
}
