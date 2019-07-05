using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Newtonsoft.Json;

namespace Microsoft.BotBuilderSamples.Dialogs
{
    public class HazardDialog : CancelAndHelpDialog
    {
        public HazardDialog()
            : base(nameof(HazardDialog))
        {
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog),
                new WaterfallStep[] {GetInitialHazardInfoAsync, GetHazardUrgencyAsync, FinalStepAsync}));
            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> GetInitialHazardInfoAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var hazardDetails = (ResponseSet.Hazard)stepContext.Options;

            hazardDetails.HazardType = (string)stepContext.Result;

            if (hazardDetails.HazardType == null)
            {
                return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("What kind of hazard would you like to report? Provide me a brief description") }, cancellationToken);
            }
            else
            {
                return await stepContext.NextAsync(hazardDetails.HazardType, cancellationToken);
            }
        }

        private async Task<DialogTurnResult> GetHazardUrgencyAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var hazardDetails = (ResponseSet.Hazard)stepContext.Options;
            hazardDetails.HazardType = (string)stepContext.Result;
            var hazardAsJson = JsonConvert.SerializeObject(hazardDetails);

            if (hazardDetails.HarzardUrgency == null)
            {
                return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text($"Thanks. So your hazard is {hazardDetails.HazardType}? How urgent is it?") }, cancellationToken);
            }
            else
            {
                var guid = Guid.NewGuid();
                var ticketId = "HAZ" + Convert.ToString(guid).ToUpper().Substring(1, 4);
                await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Thanks! I've got all the informatio I need. I'll raise this with the API team on your behalf. Your Ticket ID is: {ticketId} "), cancellationToken);
                return await stepContext.NextAsync(cancellationToken, cancellationToken);
            }
        }

        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var hazardDetails = (ResponseSet.Hazard)stepContext.Options;
            hazardDetails.HarzardUrgency = (string)stepContext.Result;
            var hazardAsJson = JsonConvert.SerializeObject(hazardDetails);
            return await stepContext.EndDialogAsync(hazardDetails, cancellationToken);
        }

    }
}