namespace System.ComponentModel.DataAnnotations;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public sealed class ResetPasswordAnnotationsAttribute : Attribute
{
    #region Properties

    /// <summary>
    /// Gets or sets the Autocomplete attribute property.
    /// </summary>
    /// <value>
    /// The autocomplete is generally used as the attribute autocomplete of html input tag.
    /// A <c>null</c> or empty string is legal, and consumers must allow for that.
    /// </value>
    public string? Autocomplete { get; set; }

    #endregion


/*
    #region Member Fields

    private Type _resourceType;
    private LocalizableString _name = new LocalizableString("Name");
    private bool? _autoGenerateField;
    private bool? _autoGenerateFilter;
    private int? _order;

    #endregion

    #region All Constructors

    /// <summary>
    /// Default constructor for DisplayAttribute. All associated string properties and methods will return <c>null</c>.
    /// </summary>
    public ResetPasswordAnnotationsAttribute()
    {
    }

    #endregion

    #region Properties

    /// <summary>
    /// Gets or sets the Name attribute property, which may be a resource key string.
    /// <para>
    /// Consumers must use the <see cref="GetName"/> method to retrieve the UI display string.
    /// </para>
    /// </summary>
    /// <remarks>
    /// The property contains either the literal, non-localized string or the resource key
    /// to be used in conjunction with <see cref="ResourceType"/> to configure a localized
    /// name for display.
    /// <para>
    /// The <see cref="GetName"/> method will return either the literal, non-localized
    /// string or the localized string when <see cref="ResourceType"/> has been specified.
    /// </para>
    /// </remarks>
    /// <value>
    /// The name is generally used as the field label for a UI element bound to the member
    /// bearing this attribute.  A <c>null</c> or empty string is legal, and consumers must allow for that.
    /// </value>
    [SuppressMessage("Microsoft.Naming", "CA1721:PropertyNamesShouldNotMatchGetMethods", Justification = "The property and method are a matched pair")]
    public string Name
    {
        get
        {
            return this._name.Value;
        }
        set
        {
            if (this._name.Value != value)
            {
                this._name.Value = value;
            }
        }
    }

    /// <summary>
    /// Gets or sets the <see cref="System.Type"/> that contains the resources for <see cref="ShortName"/>,
    /// <see cref="Name"/>, <see cref="Description"/>, <see cref="Prompt"/>, and <see cref="GroupName"/>.
    /// Using <see cref="ResourceType"/> along with these Key properties, allows the <see cref="GetShortName"/>,
    /// <see cref="GetName"/>, <see cref="GetDescription"/>, <see cref="GetPrompt"/>, and <see cref="GetGroupName"/>
    /// methods to return localized values.
    /// </summary>
    public Type ResourceType
    {
        get
        {
            return this._resourceType;
        }
        set
        {
            if (this._resourceType != value)
            {
                this._resourceType = value;

                this._name.ResourceType = value;
            }
        }
    }

    /// <summary>
    /// Gets or sets whether UI should be generated automatically to display this field. If this property is not
    /// set then the presentation layer will automatically determine whether UI should be generated. Setting this
    /// property allows an override of the default behavior of the presentation layer.
    /// <para>
    /// Consumers must use the <see cref="GetAutoGenerateField"/> method to retrieve the value, as this property getter will throw
    /// an exception if the value has not been set.
    /// </para>
    /// </summary>
    /// <exception cref="System.InvalidOperationException">
    /// If the getter of this property is invoked when the value has not been explicitly set using the setter.
    /// </exception>
    [SuppressMessage("Microsoft.Naming", "CA1721:PropertyNamesShouldNotMatchGetMethods", Justification = "The property and method are a matched pair")]
    public bool AutoGenerateField
    {
        get
        {
            if (!this._autoGenerateField.HasValue)
            {
                throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, DataAnnotationsResources.DisplayAttribute_PropertyNotSet, "AutoGenerateField", "GetAutoGenerateField"));
            }

            return this._autoGenerateField.Value;
        }
        set
        {
            this._autoGenerateField = value;
        }
    }

    /// <summary>
    /// Gets or sets whether UI should be generated automatically to display filtering for this field. If this property is not
    /// set then the presentation layer will automatically determine whether filtering UI should be generated. Setting this
    /// property allows an override of the default behavior of the presentation layer.
    /// <para>
    /// Consumers must use the <see cref="GetAutoGenerateFilter"/> method to retrieve the value, as this property getter will throw
    /// an exception if the value has not been set.
    /// </para>
    /// </summary>
    /// <exception cref="System.InvalidOperationException">
    /// If the getter of this property is invoked when the value has not been explicitly set using the setter.
    /// </exception>
    [SuppressMessage("Microsoft.Naming", "CA1721:PropertyNamesShouldNotMatchGetMethods", Justification = "The property and method are a matched pair")]
    public bool AutoGenerateFilter
    {
        get
        {
            if (!this._autoGenerateFilter.HasValue)
            {
                throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, DataAnnotationsResources.DisplayAttribute_PropertyNotSet, "AutoGenerateFilter", "GetAutoGenerateFilter"));
            }

            return this._autoGenerateFilter.Value;
        }
        set
        {
            this._autoGenerateFilter = value;
        }
    }

    /// <summary>
    /// Gets or sets the order in which this field should be displayed.  If this property is not set then
    /// the presentation layer will automatically determine the order.  Setting this property explicitly
    /// allows an override of the default behavior of the presentation layer.
    /// <para>
    /// Consumers must use the <see cref="GetOrder"/> method to retrieve the value, as this property getter will throw
    /// an exception if the value has not been set.
    /// </para>
    /// </summary>
    /// <exception cref="System.InvalidOperationException">
    /// If the getter of this property is invoked when the value has not been explicitly set using the setter.
    /// </exception>
    [SuppressMessage("Microsoft.Naming", "CA1721:PropertyNamesShouldNotMatchGetMethods", Justification = "The property and method are a matched pair")]
    public int Order
    {
        get
        {
            if (!this._order.HasValue)
            {
                throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, DataAnnotationsResources.DisplayAttribute_PropertyNotSet, "Order", "GetOrder"));
            }

            return this._order.Value;
        }
        set
        {
            this._order = value;
        }
    }

    #endregion

    #region Methods

    /// <summary>
    /// Gets the UI display string for Name.
    /// <para>
    /// This can be either a literal, non-localized string provided to <see cref="Name"/> or the
    /// localized string found when <see cref="ResourceType"/> has been specified and <see cref="Name"/>
    /// represents a resource key within that resource type.
    /// </para>
    /// </summary>
    /// <returns>
    /// When <see cref="ResourceType"/> has not been specified, the value of
    /// <see cref="Name"/> will be returned.
    /// <para>
    /// When <see cref="ResourceType"/> has been specified and <see cref="Name"/>
    /// represents a resource key within that resource type, then the localized value will be returned.
    /// </para>
    /// <para>
    /// Can return <c>null</c> and will not fall back onto other values, as it's more likely for the
    /// consumer to want to fall back onto the property name.
    /// </para>
    /// </returns>
    /// <exception cref="System.InvalidOperationException">
    /// After setting both the <see cref="ResourceType"/> property and the <see cref="Name"/> property,
    /// but a public static property with a name matching the <see cref="Name"/> value couldn't be found
    /// on the <see cref="ResourceType"/>.
    /// </exception>
    [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "This method does work using a property of the same name")]
    public string GetName()
    {
        return this._name.GetLocalizableValue();
    }

    /// <summary>
    /// Gets the value of <see cref="AutoGenerateField"/> if it has been set, or <c>null</c>.
    /// </summary>
    /// <returns>
    /// When <see cref="AutoGenerateField"/> has been set returns the value of that property.
    /// <para>
    /// When <see cref="AutoGenerateField"/> has not been set returns <c>null</c>.
    /// </para>
    /// </returns>
    [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "This method does work using a property of the same name")]
    public bool? GetAutoGenerateField()
    {
        return this._autoGenerateField;
    }

    /// <summary>
    /// Gets the value of <see cref="AutoGenerateFilter"/> if it has been set, or <c>null</c>.
    /// </summary>
    /// <returns>
    /// When <see cref="AutoGenerateFilter"/> has been set returns the value of that property.
    /// <para>
    /// When <see cref="AutoGenerateFilter"/> has not been set returns <c>null</c>.
    /// </para>
    /// </returns>
    [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "This method does work using a property of the same name")]
    public bool? GetAutoGenerateFilter()
    {
        return this._autoGenerateFilter;
    }

    /// <summary>
    /// Gets the value of <see cref="Order"/> if it has been set, or <c>null</c>.
    /// </summary>
    /// <returns>
    /// When <see cref="Order"/> has been set returns the value of that property.
    /// <para>
    /// When <see cref="Order"/> has not been set returns <c>null</c>.
    /// </para>
    /// </returns>
    /// <remarks>
    /// When an order is not specified, presentation layers should consider using the value
    /// of 10000.  This value allows for explicitly-ordered fields to be displayed before
    /// and after the fields that don't specify an order.
    /// </remarks>
    [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "This method does work using a property of the same name")]
    public int? GetOrder()
    {
        return this._order;
    }

    #endregion
*/
}
