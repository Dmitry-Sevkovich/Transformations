# Transformations

Transformations tool is used to populate files based on the preset variables.
It is mostly for population of .NET config files based on the environment those files are at.

# Use

To use the tool:

1. Compile it in Release mode.

2. Drop the Transformations.exe file in the root folder of the project.

3. In the same folder create the folder "config". This is where you are going to store all of the variables.

4. In the config folder create the file global.properties. That one will hold properties which are going to be used across all environments.

5. In the config folder create the file {env_name}.properties, where {env_name} is the name of the target environment. That file will hold the properties used on the target environment only. Any properties in the {env_name}.properties file will overwrite the properties in the from the global.properties file if the names are the same.

6. The structure of the properties files as follows:

  1. ```
      <properties>
      
        <property name="TestProperty" value="ValueForTesting"/>
        
      </properties>
      ```
  2. General rule is that each .properties file has to have a root node <properties>
  
7. Create the template file (extension .ttemplate (note double t)) for each of the config files (or any other files) you want to transform. For example, if file name is web.config, name the template file web.config.ttemplate

  1. In the .ttemplate file put ${TestProperty} in the spot, where you want it to be changed for its value. 
    
    For example, if you have the test.config.ttemplate with the following:
        
        Test123
        abc(${TestProperty})def
    
    Then, after transformation you'll get the file test.config, which will have:
    
        Test123
        abc(ValueForTesting)def

  2. To put some text in the file based on the environment use the following syntax:
  ```
      #if(local||dev)
        <settingForLocalAndDev/>
      #endif
      #if(prod)
        <settingForProd/>
      #endif
   ``` 
      In that case on local and dev environments you'll get the following:
        
        <settingForLocalAndDev/>
        
      While on prod you'll get:
      
        <settingForProd/>
        
8. To specify the environment you are in, drop the file {env_name}.environment into the same folder as Transformations.exe file. In that case you are able to run the transformations just clicking on the .exe file. Or you can specify the environment as the first command line argument, in which case the .environment file is not needed. You can also specify the working directory as the second command line argument, where the program is going to perform transformations. In that case the program is going to act as it is located in the specified directory. NOTE: If you want to specify the directory, the environment has to be specified as the first command line argument and .environment file will not be taken into consideration.
