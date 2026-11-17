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
          "WorkflowTypeId": "samplecountcoffees",
          "Name": "Sample: count the coffees",
          "IsEnabled": true,
          "IsSingleton": false,
          "DeleteFinishedWorkflows": false,
          "Variables": [
            {
              "Name": "coffee",
              "TypeName": "number",
              "DefaultValue": 0,
              "Description": "How many items of the list are Coffee."
            },
            {
              "Name": "summary",
              "TypeName": "string",
              "Description": "The result, written in Liquid.",
              "IsOutput": true
            }
          ],
          "Activities": [
            {
              "ActivityId": "started",
              "Name": "StartedByWorkflowEvent",
              "X": 20,
              "Y": 40,
              "IsStart": true,
              "Properties": {}
            },
            {
              "ActivityId": "eachitem",
              "Name": "ForEachTask",
              "X": 320,
              "Y": 40,
              "IsStart": false,
              "Properties": {
                "ActivityMetadata": {
                  "Title": "Each item"
                },
                "Enumerable": {
                  "Expression": "Tea, Coffee, Cocoa, Coffee",
                  "Syntax": "Literal"
                },
                "LoopVariableName": "item"
              }
            },
            {
              "ActivityId": "checkitem",
              "Name": "IfElseTask",
              "X": 620,
              "Y": 40,
              "IsStart": false,
              "Properties": {
                "ActivityMetadata": {
                  "Title": "Check the item"
                },
                "Condition": {
                  "Expression": "property('item') == 'Coffee'",
                  "Syntax": "JavaScript"
                }
              }
            },
            {
              "ActivityId": "countcoffee",
              "Name": "SetVariableTask",
              "X": 920,
              "Y": 40,
              "IsStart": false,
              "Properties": {
                "ActivityMetadata": {
                  "Title": "Count the coffee"
                },
                "VariableName": "coffee",
                "Value": {
                  "Expression": "variable('coffee') + 1",
                  "Syntax": "JavaScript"
                }
              }
            },
            {
              "ActivityId": "writesummary",
              "Name": "SetVariableTask",
              "X": 320,
              "Y": 320,
              "IsStart": false,
              "Properties": {
                "ActivityMetadata": {
                  "Title": "Write the summary"
                },
                "VariableName": "summary",
                "Value": {
                  "Expression": "Coffee items: {{ Workflow.Variables.coffee }}",
                  "Syntax": "Liquid"
                }
              }
            }
          ],
          "Transitions": [
            {
              "SourceActivityId": "started",
              "SourceOutcomeName": "Done",
              "DestinationActivityId": "eachitem"
            },
            {
              "SourceActivityId": "eachitem",
              "SourceOutcomeName": "Iterate",
              "DestinationActivityId": "checkitem"
            },
            {
              "SourceActivityId": "eachitem",
              "SourceOutcomeName": "Done",
              "DestinationActivityId": "writesummary"
            },
            {
              "SourceActivityId": "checkitem",
              "SourceOutcomeName": "True",
              "DestinationActivityId": "countcoffee"
            },
            {
              "SourceActivityId": "checkitem",
              "SourceOutcomeName": "False",
              "DestinationActivityId": "eachitem"
            },
            {
              "SourceActivityId": "countcoffee",
              "SourceOutcomeName": "Done",
              "DestinationActivityId": "eachitem"
            }
          ]
        }
      ]
    }
  ]
}
""";
}
