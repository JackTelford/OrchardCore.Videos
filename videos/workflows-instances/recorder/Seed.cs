namespace Recorder;

/// <summary>The workflows that PrepareAsync imports (the JSON of the recording plan).</summary>
internal static class Seed
{
    public const string Json = """
{
  "name": "workflow-instances-video-seed",
  "displayName": "Workflow instances video: seed",
  "description": "Two workflows with fake data for the Workflow Instances video.",
  "author": "The Orchard Core Team",
  "website": "https://orchardcore.net",
  "version": "1.0.0",
  "issetuprecipe": false,
  "categories": [],
  "tags": [],
  "steps": [
    {
      "name": "WorkflowType",
      "data": [
        {
          "WorkflowTypeId": "orderfollowup",
          "Name": "Order follow-up",
          "IsEnabled": true,
          "IsSingleton": false,
          "DeleteFinishedWorkflows": false,
          "Variables": [
            {
              "Name": "orderNumber",
              "TypeName": "string",
              "Description": "The order to follow up.",
              "IsInput": true
            }
          ],
          "Activities": [
            {
              "ActivityId": "orderreceived",
              "Name": "StartedByWorkflowEvent",
              "X": 40,
              "Y": 40,
              "IsStart": true,
              "Properties": {
                "ActivityMetadata": {
                  "Title": "Order received"
                }
              }
            },
            {
              "ActivityId": "checkstock",
              "Name": "HttpRequestTask",
              "X": 380,
              "Y": 40,
              "IsStart": false,
              "Properties": {
                "ActivityMetadata": {
                  "Title": "Check stock"
                },
                "HttpMethod": "GET",
                "Url": {
                  "Expression": "http://127.0.0.1:5099/stock",
                  "Syntax": "Literal"
                },
                "HttpResponseCodes": "200"
              }
            },
            {
              "ActivityId": "stockconfirmed",
              "Name": "LogTask",
              "X": 720,
              "Y": 40,
              "IsStart": false,
              "Properties": {
                "ActivityMetadata": {
                  "Title": "Stock confirmed"
                },
                "Text": {
                  "Expression": "The order is in stock.",
                  "Syntax": "Literal"
                }
              }
            }
          ],
          "Transitions": [
            {
              "SourceActivityId": "orderreceived",
              "SourceOutcomeName": "Done",
              "DestinationActivityId": "checkstock"
            },
            {
              "SourceActivityId": "checkstock",
              "SourceOutcomeName": "200",
              "DestinationActivityId": "stockconfirmed"
            }
          ]
        },
        {
          "WorkflowTypeId": "paymentfollowup",
          "Name": "Payment follow-up",
          "IsEnabled": true,
          "IsSingleton": false,
          "DeleteFinishedWorkflows": false,
          "Variables": [
            {
              "Name": "orderNumber",
              "TypeName": "string",
              "Description": "The order waiting for its payment.",
              "IsInput": true
            }
          ],
          "Activities": [
            {
              "ActivityId": "paymentrequested",
              "Name": "StartedByWorkflowEvent",
              "X": 40,
              "Y": 40,
              "IsStart": true,
              "Properties": {
                "ActivityMetadata": {
                  "Title": "Payment requested"
                }
              }
            },
            {
              "ActivityId": "waitforpayment",
              "Name": "SignalEvent",
              "X": 380,
              "Y": 40,
              "IsStart": false,
              "Properties": {
                "ActivityMetadata": {
                  "Title": "Wait for payment"
                },
                "SignalName": {
                  "Expression": "payment-received",
                  "Syntax": "Literal"
                }
              }
            },
            {
              "ActivityId": "paymentreceived",
              "Name": "LogTask",
              "X": 720,
              "Y": 40,
              "IsStart": false,
              "Properties": {
                "ActivityMetadata": {
                  "Title": "Payment received"
                },
                "Text": {
                  "Expression": "The payment arrived.",
                  "Syntax": "Literal"
                }
              }
            }
          ],
          "Transitions": [
            {
              "SourceActivityId": "paymentrequested",
              "SourceOutcomeName": "Done",
              "DestinationActivityId": "waitforpayment"
            },
            {
              "SourceActivityId": "waitforpayment",
              "SourceOutcomeName": "Done",
              "DestinationActivityId": "paymentreceived"
            }
          ]
        }
      ]
    }
  ]
}
""";
}
