using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security;
using Hardcodet.Commons;
using Hardcodet.NetDrives.Platform;
using Hardcodet.NetDrives.Platform.Core.Shares;
using Hardcodet.NetDrives.Platform.Interop;
using Hardcodet.NetDrives.Properties;

namespace Hardcodet.NetDrives.ViewModel
{
  /// <summary>
  /// Login options as presented through the UI.
  /// </summary>
  public enum LoginType
  {
    /// <summary>
    /// No password is required.
    /// </summary>
    NoPassword,
    /// <summary>
    /// Prompts for credentials - this disables auto-connections.
    /// </summary>
    PromptForCredentials,
    /// <summary>
    /// Requires a valid password and user ID.
    /// </summary>
    PasswordSupplied
  }


  /// <summary>
  /// Provides a VM wrapper for a given <see cref="ShareConfiguration"/>
  /// instance.
  /// </summary>
  public class ConfigurationWrapper : SimpleObject, IDataErrorInfo
  {
    /// <summary>
    /// The underlying configuration.
    /// </summary>
    public IShareConfiguration Configuration { get; private set; }


    #region Configuration Properties

    private string name;
    private char? driveLetter;
    private string volumeLabel;
    private bool autoConnect;
    private string networkLocation;
    private string userName;
    private SecureString password;

    /// <summary>
    /// A descriptive (user) name for the share.
    /// </summary>
    public string Name
    {
      get { return name; }
      set
      {
        name = value;
        OnPropertyChanged(() => Name);
      }
    }

    /// <summary>
    /// The drive letter to be registered for this share, if any.
    /// Exposes null values as <see cref="char.MinValue"/>.
    /// </summary>
    public char? DriveLetter
    {
      get { return driveLetter; }
      set
      {
        if (value == char.MinValue) value = null;

        driveLetter = value;
        OnPropertyChanged(() => DriveLetter);
      }
    }


    /// <summary>
    /// A label to be registered for this share, which causes file managers
    /// to display the share with this label.
    /// </summary>
    public string VolumeLabel
    {
      get { return volumeLabel; }
      set
      {
        volumeLabel = value;
        OnPropertyChanged(() => VolumeLabel);
      }
    }

    /// <summary>
    /// Whether a connection to the share should be automatically
    /// established on application startup. Defaults to false.
    /// </summary>
    public bool AutoConnect
    {
      get { return autoConnect; }
      set
      {
        autoConnect = value;
        OnPropertyChanged(() => AutoConnect);
        OnPropertyChanged(() => LoginType);
      }
    }

    /// <summary>
    /// The remote (network) address of the share.
    /// </summary>
    public string NetworkLocation
    {
      get { return networkLocation; }
      set
      {
        networkLocation = value;
        OnPropertyChanged(() => NetworkLocation);
        OnPropertyChanged(() => IsHostAddress);
        OnPropertyChanged(() => IsDriveLetterAllowed);
      }
    }

    /// <summary>
    /// The user name to be used when connecting to the share
    /// </summary>
    public string UserName
    {
      get { return userName; }
      set
      {
        userName = value;
        OnPropertyChanged(() => UserName);
      }
    }

    /// <summary>
    /// The password to be used when connecting to the share.
    /// </summary>
    public SecureString Password
    {
      get { return password; }
      set
      {
        password = value;
        OnPropertyChanged(() => Password);
      }
    }

    #endregion


    #region LoginType

    /// <summary>
    /// The login to be performed.
    /// </summary>
    private LoginType loginType;


    /// <summary>
    /// The login to be performed.
    /// </summary>
    public LoginType LoginType
    {
      get { return loginType; }
      set
      {
        //ignore if values are equal
        if (value == loginType) return;

        loginType = value;
        OnPropertyChanged(() => LoginType);
        OnPropertyChanged(() => AutoConnect);
        OnPropertyChanged(() => IsAutoConnectEnabled);
        OnPropertyChanged(() => IsValidPassword);
        OnPropertyChanged(() => IsPasswordRequired);
      }
    }

    #endregion


    /// <summary>
    /// Validates the <see cref="NetworkLocation"/>
    /// property.
    /// </summary>
    public bool IsValidNetworkLocation
    {
      get
      {
        Uri uri;
        bool status = Uri.TryCreate(NetworkLocation, UriKind.Absolute, out uri);
        if (!status) return false;
        return uri.IsFile && uri.IsUnc;
      }
    }

    /// <summary>
    /// Checks whether the <see cref="ShareConfiguration.NetworkLocation"/>
    /// address refers to a host address or not. Also returns false if
    /// the address cannot be interpreted at all.
    /// </summary>
    public bool IsHostAddress
    {
      get
      {
        Uri uri;
        bool status = Uri.TryCreate(NetworkLocation, UriKind.Absolute, out uri);
        if (!status) return false;

        //make sure we operate on the local path
        status = Uri.TryCreate(uri.LocalPath, UriKind.Absolute, out uri);
        if (!status) return false;

        return uri.Segments.Length == 1;
      }
    }

    
    /// <summary>
    /// Indicates whether a drive letter can be assigned
    /// for a given host address.
    /// </summary>
    public bool IsDriveLetterAllowed
    {
      get { return !IsHostAddress; }
    }


    /// <summary>
    /// Checks whether auto connection is a valid alternative.
    /// </summary>
    public bool IsAutoConnectEnabled
    {
      get { return LoginType != LoginType.PromptForCredentials; }
    }


    /// <summary>
    /// Checks whether the user should supply a password or not.
    /// </summary>
    public bool IsPasswordRequired
    {
      get { return LoginType == LoginType.PasswordSupplied; }
    }


    /// <summary>
    /// Checks if the password is valid.
    /// </summary>
    public bool IsValidPassword
    {
      get
      {
        return LoginType != LoginType.PasswordSupplied ||
               !Password.IsNullOrEmpty();
      }
    }


    /// <summary>
    /// Creates the wrapper for a given configuration.
    /// </summary>
    /// <param name="configuration">The wrapped configuration.</param>
    public ConfigurationWrapper(IShareConfiguration configuration)
    {
      Ensure.ArgumentNotNull(() => configuration);
      Configuration = configuration;

      //init properties
      Name = configuration.Name;
      VolumeLabel = configuration.VolumeLabel;
      AutoConnect = configuration.AutoConnect;
      NetworkLocation = configuration.NetworkLocation;
      UserName = configuration.UserName;
      Password = configuration.Password;

      //reset drive letter no matter the underlying configuration if it's not valid
      DriveLetter = IsHostAddress ? null : configuration.DriveLetter;

      //determine login type
      if (configuration.Login == LoginStrategy.Prompt)
      {
        loginType = LoginType.PromptForCredentials;
      }
      else
      {
        loginType = configuration.Password.IsNullOrEmpty()
                        ? LoginType.NoPassword
                        : LoginType.PasswordSupplied;
      }
    }



    /// <summary>
    /// Updates the underlying configuration.
    /// </summary>
    public void UpdateConfiguration()
    {
      //init properties
      Name = Configuration.Name = Name;

      if (driveLetter == char.MinValue) driveLetter = null;
      Configuration.DriveLetter = IsDriveLetterAllowed ? DriveLetter : null;
      
      Configuration.VolumeLabel = VolumeLabel;
      Configuration.AutoConnect = AutoConnect;
      Configuration.NetworkLocation = NetworkLocation;
      Configuration.UserName = UserName;
      
      if (LoginType == LoginType.NoPassword)
      {
        Configuration.Password = null;
      }
      else
      {
        Configuration.Password = Password;
      }
      

      //determine login type
      if (LoginType == LoginType.PromptForCredentials)
      {
        Configuration.Login = LoginStrategy.Prompt;
      }
      else
      {
        Configuration.Login = LoginStrategy.Automatic;
      }
    }


    #region Validation

    /// <summary>
    /// Gets the error message for the property with the given name.
    /// </summary>
    /// <returns>
    /// The error message for the property. The default is an empty string ("").
    /// </returns>
    /// <param name="columnName">
    /// The name of the property whose error message to get. 
    /// </param>
    public string this[string columnName]
    {
      get
      {
        switch(columnName)
        {
          case "NetworkLocation":
            if (String.IsNullOrEmpty(NetworkLocation)) return Resources.Validation_Error_If_Missing_Network_Address;
            if (!IsValidNetworkLocation) return Resources.Validation_Error_If_Invalid_Network_Address;
            break;
          case "Name":
            if (String.IsNullOrEmpty(Name)) return Resources.Validation_Error_If_Missing_Share_Name;
            break;
          case "Password":
            if (Password.IsNullOrEmpty() && IsPasswordRequired) return Resources.Warning_About_Missing_Password_If_Auto_Login_Was_Configured;
            break;
        }

        return String.Empty;
      }
    }


    /// <summary>
    /// Not needed here.
    /// </summary>
    public string Error
    {
      get
      {
        return IsValid ? null : "Errors detected";
      }
    }


    /// <summary>
    /// Validates whether the item as a whole is valid or not.
    /// </summary>
    public bool IsValid
    {
      get
      {
        return String.IsNullOrEmpty(this["NetworkLocation"]) &&
               String.IsNullOrEmpty(this["Name"]) &&
               String.IsNullOrEmpty(this["Password"]);
      }
    }

    #endregion


    #region GetDriveLetters

    /// <summary>
    /// Determines available drive letters to be offered
    /// for the share.
    /// </summary>
    /// <returns>A list of drive letter characters.</returns>
    public IEnumerable<char> GetDriveLetters()
    {
      //determine available drive letters (as nullables
      var driveLetters = NetworkUtil.GetAvailableDriveLetters().ToList();
      if (Configuration.DriveLetter.HasValue && !driveLetters.Contains(Configuration.DriveLetter.Value))
      {
        //make sure the current letter is part of the list...
        driveLetters.Add(Configuration.DriveLetter.Value);
      }

      //also assign a empty value (null causes exception in 3.5 SP1)
      driveLetters.Insert(0, char.MinValue);
      return driveLetters;
    }

    #endregion

  }
}