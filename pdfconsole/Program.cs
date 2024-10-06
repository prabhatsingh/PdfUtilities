using ImageLibrary;
using Libraries.CommonUtilities;
using Libraries.CommonUtilities.Models;
using PdfLibrary;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using XpsLibrary;

namespace PdfConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            //PrintTest(args);
            //PdfLibrary.MagickHelper.ConvertToScanned(args.First());

            Console.Title = "Pdf Utilities by Prabhat Singh";

            ShellHelper.AddMenuItem();

            if (args.Length == 0)
                ShowWelcomeMessage();
            else
            {
                ActionUtilities.ActionInfo actinf = PrepareAction(args);

                var result = StartTaskAsync(actinf).GetAwaiter().GetResult();

                ConsoleUtilities.PrintLine("Job completed successfully!", ConsoleColor.Green);
            }

            Console.ReadKey();
        }

        private static async Task<string> StartTaskAsync(ActionUtilities.ActionInfo actinf)
        {
            ConsoleUtilities.PrintLine("Performing selected action", ConsoleColor.DarkYellow);

            var progress = new Progress<CommonUtilities.Models.ConsoleProp>(percent =>
            {
                Console.Write("\r" + percent + "%");
            });

            await Task.Run(() => PerformAction(actinf, progress));

            System.Threading.Timer closetimer = new System.Threading.Timer(Closeapp, null, 2000, 5000);

            return "Success";
        }

        private static void PerformAction(ActionUtilities.ActionInfo actioninfo, IProgress<CommonUtilities.Models.ConsoleProp> progress)
        {
            if (actioninfo.ActionTargetType == FileType.PDF)
            {
                actioninfo.ActionTarget.ForEach(file =>
                {
                    if (new ITextHelper().IsXFA(file.Filepath))
                    {
                        //progress.Report(new CommonUtilities.Models.ConsoleProp { concolor = ConsoleColor.Green, message = "The file {0} contains forms, printing it to XPS", p = new string[] { file.Filename } });
                        ConsoleUtilities.PrintLine("The file {0} contains forms, printing it to XPS", ConsoleColor.Green, file.Filename);
                        AdobeHelper.PrintPdf(file.Filepath);
                    }
                });

                actioninfo.ActionTarget.RemoveAll(f => new ITextHelper().IsXFA(f.Filepath));

                switch (actioninfo.ActionType)
                {
                    case ActionType.PDFTOIMAGE:
                        new PdfActions(new MagickHelper()).Run(actioninfo);
                        break;
                    case ActionType.LOOKSCANNED:
                        new PdfActions(new MagickHelper()).Run(actioninfo);
                        break;
                    case ActionType.SPLIT:
                    case ActionType.ROTATECW:
                    case ActionType.ROTATECCW:
                    case ActionType.ROTATE180:
                    case ActionType.MERGE:
                        new PdfActions(new ITextHelper()).Run(actioninfo);
                        break;
                }
            }
            else if (actioninfo.ActionTargetType == FileType.IMAGE)
            {
                switch (actioninfo.ActionType)
                {
                    case ActionType.IMAGETOPDF:
                        new PdfActions(new ITextHelper()).Run(actioninfo);
                        break;
                    case ActionType.OPTIMIZE:
                    case ActionType.ROTATECW:
                    case ActionType.ROTATECCW:
                    case ActionType.ROTATE180:
                        new ImageActions(new ImageProcessorHelper()).Run(actioninfo);
                        break;
                    case ActionType.ROTATEOTH:
                        System.Windows.Forms.Form form = new RotationForm(actioninfo);
                        form.ShowDialog();
                        break;
                }
            }
            else if (actioninfo.ActionTargetType == FileType.XPS)
            {
                switch (actioninfo.ActionType)
                {
                    case ActionType.XPSTOIMAGE:
                        actioninfo.ActionTarget.ForEach(file => new XpsHelper().XpsToImage(file.Filepath));
                        break;
                    case ActionType.XPSTOPDF:
                        actioninfo.ActionTarget.ForEach(file => new ITextHelper().ImageToPdf(new XpsHelper().XpsToImage(file.Filepath, true), file.Filepath));
                        break;
                }
            }
            else if (actioninfo.ActionTargetType == FileType.COMBINED)
            {
                switch (actioninfo.ActionType)
                {
                    case ActionType.COMBINE:
                        new PdfActions(new ITextHelper()).Run(actioninfo);
                        break;
                }
            }
        }

        private static ActionUtilities.ActionInfo PrepareAction(string[] args)
        {
            var actionInfo = new ActionUtilities.ActionInfo();
            args.ToList().ForEach(f => actionInfo.ActionTarget.Add(new FileUtilities.FileDetails(f)));
            actionInfo.ActionTarget = actionInfo.ActionTarget.OrderBy(f => f.Filename).ToList();
            actionInfo.AskUser();
            return actionInfo;
        }

        private static void Closeapp(object state)
        {
            Environment.Exit(0);
        }

        private static void ShowWelcomeMessage()
        {
            string AssemblyVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            string AssemblyTitle = Assembly.GetExecutingAssembly().GetName().Name;

            Console.WriteLine();
            ConsoleUtilities.PrintLine($"{AssemblyTitle} {AssemblyVersion}\nCopyright © {DateTime.Now.Year} Prabhat Singh\n", ConsoleColor.Green);
            ConsoleUtilities.PrintLine("Select pdf or image files in explorer and click on \nthe Pdf Utilities link in context menu's send to!!", ConsoleColor.Yellow);
        }
    }
}
