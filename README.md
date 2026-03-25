# Bible Aligner using Open Ai API
The main objective of this project is to be able to use AI to align the Hebrew Bible (and eventually Greek NT) with any Bible translation in order to create an interlinear or to tag the translation with Strong’s numbers.<br>
All the code is written in c# using Visual Studio Community 2026. This repository contains a number of experimental projects; however, the main project is OpenAIAligner. It includes all the needed Hebrew text; however, the translated Bible must be provided in a text file, with a verse per line in the format:<br><br>
&nbsp;&nbsp;Gen 1:1 In the beginning God created the heavens and the earth. <br><br>
In order to use any of the code with Open AI API, one must obtain an API key. <br><br>
## OpenAIAligner Project
This is a Windows Forms project with menus to allow selecting a range of Bible verses or a set of scattered verses for alignment. The program outputs 3 files in a folder named Alignments under the run folder.<br>
<name>timestamp.txt<br>
Prompt<name>timestamp(#).txt<br>
Result<name>timestamp(#).json<br>
