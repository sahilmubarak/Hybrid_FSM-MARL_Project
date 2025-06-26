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