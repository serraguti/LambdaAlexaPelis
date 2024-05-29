using Alexa.NET.Request;
using Alexa.NET.Request.Type;
using Alexa.NET.Response;
using Amazon.Lambda.Core;
using LambdaAlexaPelis.Models;
using LambdaAlexaPelis.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: 
    LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace LambdaAlexaPelis;

public class Function
{
    ILambdaLogger log;

    public async Task<SkillResponse>
            FunctionHandler(SkillRequest input, ILambdaContext context)
    {
        ServiceApiPeliculas service = new ServiceApiPeliculas();
        SkillResponse response = new SkillResponse();
        response.Response = new ResponseBody();
        response.Response.ShouldEndSession = false;
        IOutputSpeech innerResponse = null;
        this.log = context.Logger;
        log.LogLine($"Skill Request Object:" 
            + JsonConvert.SerializeObject(input));
        if (input.GetRequestType() == typeof(LaunchRequest))
        {
            innerResponse = new PlainTextOutputSpeech();
            (innerResponse as PlainTextOutputSpeech).Text =
                "Soy tu Alexa privado.  Pideme una película...¿Qué número de Pelicula quieres?";
        }
        else if (input.GetRequestType() == typeof(IntentRequest))
        {
            var intentRequest = (IntentRequest)input.Request;
            if (intentRequest.Intent.Name == "preguntastontas")
            {
                log.LogLine("PIDIENDO DATOS!!!!!!!");
                string slotJson = JsonConvert.SerializeObject(intentRequest.Intent.Slots);
                int idpelicula = GetSlotValue(slotJson);
                log.LogLine($"Id personaje: " + idpelicula);
                log.LogLine($"Slots Goonies: " + slotJson);
                Pelicula peli = await service.FindPelicula(idpelicula);
                if (peli != null)
                {
                    innerResponse = new PlainTextOutputSpeech();
                    (innerResponse as PlainTextOutputSpeech).Text =
                        peli.Argumento;
                }
                else
                {
                    innerResponse = new PlainTextOutputSpeech();
                    (innerResponse as PlainTextOutputSpeech).Text =
                        "No he encontrado a tu Peli " + idpelicula;
                }
            }
            else
            {
                innerResponse = new PlainTextOutputSpeech();
                (innerResponse as PlainTextOutputSpeech).Text =
                    "Ni idea de lo que me hablas";
            }
        }
        else
        {
            innerResponse = new PlainTextOutputSpeech();
            (innerResponse as PlainTextOutputSpeech).Text =
                "Ni idea de lo que me hablas, en else";
        }

        response.Response.OutputSpeech = innerResponse;
        response.Version = "1.0";
        return response;
    }
    private int GetSlotValue(string dataJson)
    {
        //    string dataJson = "{ 'idpersonaje': { 'name': 'idpersonaje', " +
        //"'value': '2', 'confirmationStatus': 'NONE', 'source': 'USER', " +
        //"'slotValue': { 'type': 'Simple', 'value': '2' }}}";
        var jsonObject = JObject.Parse(dataJson);
        var data = (JObject)jsonObject["idpelicula"];
        var nombre = (string)data["name"];
        var id = (string)data["value"];
        return int.Parse(id);
    }

    private string GetSlotValueString(string dataJson)
    {
        try
        {
            var jsonObject = JObject.Parse(dataJson);
            var data = (JObject)jsonObject["idpelicula"];
            log.LogLine($"Data " + data);
            var nombre = (string)data["name"];
            log.LogLine($"nombre " + nombre);
            var id = (string)data["value"];
            log.LogLine($"Id " + id);
            return "Nombre " + nombre + ", Id: " + id;
        }
        catch (Exception ex)
        {
            log.LogLine($"Error Gordo... " + ex);
            return ex.ToString();
        }
    }
}
