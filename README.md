# Bible Aligner using Open Ai API
The main objective of this project is to be able to use AI to align the Hebrew Bible (and eventually Greek NT) with any Bible translation in order to create an interlinear or to tag the translation with Strong’s numbers.<br>
All the code is written in c# using Visual Studio Community 2026. This repository contains a number of experimental projects; however, the main project is OpenAIAligner. It includes all the needed Hebrew text; however, the translated Bible, called "target", must be provided in a text file, with a verse per line in the format:<br><br>
  Gen 1:1 In the beginning God created the heavens and the earth. <br><br>
In order to use any of the code with Open AI API, one must obtain an API key. <br>
## OpenAIAligner Project
This is a Windows Forms project with menus to allow selecting a range of Bible verses or a set of scattered verses for alignment. The program outputs 3 files in a folder named Alignments under the run folder.<br>
* &lt;name&gt;timestamp.txt<br>
* Prompt&lt;name&gt;timestamp(#).txt<br>
* Result&lt;name&gt;timestamp(#).json<br>
Where:
<table>
<tr><td><b>&lt;name&gt;</b></td><td>is a name constructed by the program from the slected verse references.</td></tr>
<tr><td><b>Prompt&lt;name&gt;timestamp(#).txt</b></td><td> text file containing the prompt(s) sent to gpt. gpt may not perform well with a very large number of verses. Therefore in the Settings menu, a maximum number of verses per prompt is set. The default is 10. Therefore, if 27 verses are selected, the program sends 3 prompts containing 10, 10 and 7 verses. Each prompt is saved in a file with the # representing the prompt number (0, 1 and 3).</td></tr>
<tr><td><b>Result&lt;name&gt;timestamp(#).json</b></td><td>the response received for each prompt</td></tr>
<tr><td><b>timestamp</b></td><td>timestamp of when the result is received.</td></tr>
<tr><td><b>&lt;name&gt;timestamp.txt</b></td><td>a tab separated values file contains the combined parsing of all the results. It contains three columns: the target word(s), their Strong's number(s), the Hebrew word(s), a note by AI explaining the rationale for the alignment</td></tr>
</table>

