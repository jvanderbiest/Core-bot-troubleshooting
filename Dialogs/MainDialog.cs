// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Microsoft.BotBuilderSamples.Dialogs
{
    internal class ResponseSet
    {
        public class Hazard
        {
            public string HarzardUrgency { get; set; }
            public string HazardType { get; set; }
        }
    }

    public class MainDialog : ComponentDialog
    {

        protected readonly IConfiguration Configuration;
        protected readonly ILogger Logger;

        public MainDialog(IConfiguration configuration, ILogger<MainDialog> logger)
            : base(nameof(MainDialog))
        {
            Configuration = configuration;
            Logger = logger;

            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new BookingDialog());
            AddDialog(new HazardDialog());
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                MainStepAsync,
                EndOfMainDialogAsync
            }));

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> EndOfMainDialogAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            await stepContext.Context.SendActivityAsync(MessageFactory.Text("Ok, I think we're all done with that. Can I do anything else to help?"), cancellationToken);
            return await stepContext.EndDialogAsync(null, cancellationToken);
        }

        private async Task<DialogTurnResult> MainStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            //string what_i_got = await LuisHelper.ExecuteMyLuisQuery(Configuration, Logger, stepContext.Context, cancellationToken);

            string what_i_got = stepContext.Context.Activity.Text;

            //await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text($"Hi! My name is Sorella. Your intent was {what_i_got}") }, cancellationToken);

            switch (what_i_got)
            {
                case "Hazard":
                    var hazardDetails = new ResponseSet.Hazard();
                    return await stepContext.BeginDialogAsync(nameof(HazardDialog), hazardDetails, cancellationToken);
                case "Greeting":
                    await stepContext.Context.SendActivityAsync(MessageFactory.Text("Hi there! :). What can I help you with today? "), cancellationToken);
                    return await stepContext.EndDialogAsync(null, cancellationToken);
                default:
                    return await stepContext.NextAsync(null, cancellationToken);
            }
        }
    }
}
