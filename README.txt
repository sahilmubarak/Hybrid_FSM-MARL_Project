You first need to install the following

Requirements:
Unity (2022.3.20f1).
ML-Agents Toolkit (v0.30.0).
Python 3.9.13.

Getting it to work:

After installing unit, go to 
1. Unity Hub > Add > Add project from disk. Then select the downloaded location of this folder.
Unity will recognise the folder and create the required files to make it usable.
2. After the project opens in unity, create a python virtual environment in the Project Folder
3. Now your Project should be set up, Restart Unity and hit play, the sim should run.

To train and monitor the MARL brains.
 
1. **Open Command Prompt**  
   Press `Win + R`, type `cmd`, and hit Enter.

2. **Navigate to the Project Directory**  
   ```
   cd /d D:\UnityProjects\First ML Project
   ```

3. **Activate the Virtual Environment**  
   ```
   venv\Scripts\activate
   ```

4. **Check ML-Agents Installation (Optional)**  
   ```
   mlagents-learn --help
   ```

5. **Train the Model**  
   ```
   mlagents-learn config\trainer_config.yaml --run-id=<your_run_id>
   ```
   Replace `<your_run_id>` with a unique name for this training session.
   
   mlagents-learn config/MARL_Brain.yaml --run-id=goal_shopper_test

6. **Run the Trained Model (Inference Mode)**  
   ```
   mlagents-learn config\trainer_config.yaml --run-id=<your_run_id> --resume --inference
   ```

7. **Deactivate the Virtual Environment (When Done)**  
   ```
   deactivate

TO visualize

Step 2,3
then 

tensorboard --logdir results

then go to port

   ```
