namespace Recorder;

/// <summary>The workflows that PrepareAsync imports (the JSON of the recording plan).</summary>
internal static class Seed
{
    public const string Json = """
{
  "steps": [
    {
      "name": "WorkflowType",
      "data": [
        {
          "WorkflowTypeId": "stockcheck",
          "Name": "Stock check",
          "IsEnabled": true,
          "IsSingleton": false,
          "DeleteFinishedWorkflows": false,
          "Variables": [
            {
              "Name": "stockBody",
              "TypeName": "string",
              "Description": "The body of the response of the stock API, bound to the Body output of the HTTP Request task."
            }
          ],
          "Activities": [
            {
              "ActivityId": "stockrequested",
              "Name": "HttpRequestEvent",
              "X": 40,
              "Y": 40,
              "IsStart": true,
              "Properties": {
                "HttpMethod": "POST",
                "ValidateAntiforgeryToken": false,
                "TokenLifeSpan": 0
              }
            },
            {
              "ActivityId": "readproduct",
              "Name": "ScriptTask",
              "X": 380,
              "Y": 40,
              "IsStart": false,
              "Properties": {
                "AvailableOutcomes": [
                  "Done"
                ],
                "Script": {
                  "Expression": "// The body of the request, parsed as JSON.\nconst request = JSON.parse(readBody());\n\n// Keep the product code for the activities that follow.\nsetProperty('sku', request.sku);\nsetOutcome('Done');"
                }
              }
            },
            {
              "ActivityId": "callstockapi",
              "Name": "HttpRequestTask",
              "X": 720,
              "Y": 40,
              "IsStart": false,
              "Properties": {
                "Url": {
                  "Expression": "http://localhost:5080/{{ Workflow.Properties.sku }}.json"
                },
                "HttpMethod": "GET",
                "HttpResponseCodes": "200, 404",
                "OutputBindings": {
                  "Body": "stockBody"
                }
              }
            },
            {
              "ActivityId": "replyfound",
              "Name": "HttpResponseTask",
              "X": 550,
              "Y": 330,
              "IsStart": false,
              "Properties": {
                "HttpStatusCode": 200,
                "ContentType": {
                  "Expression": "application/json"
                },
                "Content": {
                  "Expression": "{{ Workflow.Variables.stockBody }}"
                }
              }
            },
            {
              "ActivityId": "replynotfound",
              "Name": "HttpResponseTask",
              "X": 890,
              "Y": 330,
              "IsStart": false,
              "Properties": {
                "HttpStatusCode": 404,
                "ContentType": {
                  "Expression": "text/plain"
                },
                "Content": {
                  "Expression": "No product has this product code."
                }
              }
            }
          ],
          "Transitions": [
            {
              "SourceActivityId": "stockrequested",
              "SourceOutcomeName": "Done",
              "DestinationActivityId": "readproduct"
            },
            {
              "SourceActivityId": "readproduct",
              "SourceOutcomeName": "Done",
              "DestinationActivityId": "callstockapi"
            },
            {
              "SourceActivityId": "callstockapi",
              "SourceOutcomeName": "200",
              "DestinationActivityId": "replyfound"
            },
            {
              "SourceActivityId": "callstockapi",
              "SourceOutcomeName": "404",
              "DestinationActivityId": "replynotfound"
            }
          ]
        }
      ]
    }
  ]
}
""";
}
