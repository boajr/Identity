using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text.Encodings.Web;

namespace Boa.Identity.UI.TagHelpers;

[HtmlTargetElement("resetpasswordservices")]
public class ResetPasswordServicesTagHelper : TagHelper
{
    internal static UIFramework Framework { get; set; } = UIFramework.Bootstrap5;

    private readonly IHtmlGenerator _htmlGenerator;
    private readonly IModelMetadataProvider _metadataProvider;
    private readonly IEnumerable<IResetPasswordService> _resetPasswordServices;

    /// <summary>
    /// Gets the <see cref="Rendering.ViewContext"/> of the executing view.
    /// </summary>
    [HtmlAttributeNotBound]
    [ViewContext]
    public ViewContext ViewContext { get; set; } = default!;

    /// <summary>
    /// An expression to be evaluated against the current model.
    /// </summary>
    [HtmlAttributeName("asp-for")]
    public ModelExpression For { get; set; } = default!;

    /// <summary>
    /// An expression to be evaluated against the current model.
    /// </summary>
    [HtmlAttributeName("data")]
    public object? Data { get; set; }



    public ResetPasswordServicesTagHelper(IHtmlGenerator htmlGenerator,
                                          IModelMetadataProvider metadataProvider,
                                          IEnumerable<IResetPasswordService> resetPasswordServices)
    {
        _htmlGenerator = htmlGenerator;
        _metadataProvider = metadataProvider;
        _resetPasswordServices = resetPasswordServices;
    }

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        if (output == null)
        {
            throw new ArgumentNullException(nameof(output));
        }

        // fa in modo che il tag non venga scritto
        output.TagName = null;

        // se non ho servizi non scrivo niente ed esco
        int serviceCount = _resetPasswordServices.Count();
        if (serviceCount == 0)
        {
            output.SuppressOutput();
            return;
        }

        // preleva la voce selezionata nel combo box
        string? method = For.Model != null ? For.Model.ToString() : _resetPasswordServices.First().ServiceName;

        // crea il select (o il campo nascosto) contenente l'elenco dei servizi disponibili
        if (serviceCount > 1)
        {
            List<SelectListItem> items = new();
            foreach (var service in _resetPasswordServices)
            {
                items.Add(new SelectListItem(service.ServiceName, service.ServiceName, service.ServiceName == method));
            }

            await AppendSelectElement(For, items, null, true, context, output).ConfigureAwait(false);
        }
        else
        {
            output.Content.AppendHtml($"{Environment.NewLine}<input type=\"hidden\" name=\"");
            output.Content.Append(For.Name);
            output.Content.AppendHtml("\" value=\"");
            output.Content.Append(_resetPasswordServices.First().ServiceName);
            output.Content.AppendHtml("\">");
        }

        // processa tutti i servizi di reset password registrati
        Type? dataType = Data?.GetType();
        foreach (IResetPasswordService service in _resetPasswordServices)
        {
            // per ogni proprietà nel data model, aggiunge il box di input (o checkbox, combo ecc)
            bool show = service.ServiceName == method;
            foreach (PropertyInfo prop in service.DataModelType.GetProperties())
            {
                ModelMetadata modelMetadata = _metadataProvider.GetMetadataForProperty(service.DataModelType, prop.Name);
                object? value = show ? dataType?.GetProperty(prop.Name)?.GetValue(Data) : null;
                ModelExpression modelExpression = new($"Input.{service.ServiceName}.{prop.Name}", new ModelExplorer(_metadataProvider, modelMetadata, value));

                if (prop.PropertyType == typeof(bool))
                {
                    await AppendCheckboxElement(modelExpression, service.ServiceName, show, context, output).ConfigureAwait(false);
                    continue;
                }

                await AppendInputElement(modelExpression, service.ServiceName, show, context, output).ConfigureAwait(false);
            }
        }

        // se serve, aggiunge lo script per cambiare metodo
        if (serviceCount > 1)
        {
            output.Content.AppendHtml($"{Environment.NewLine}<script>{Environment.NewLine}    document.getElementById('");
            output.Content.Append(TagBuilder.CreateSanitizedId(For.Name, "_"));
            output.Content.AppendHtml($"').onchange = function() {{{Environment.NewLine}        var elems = document.querySelectorAll('[boa-group]');{Environment.NewLine}"
                + $"        for (var i=0; i<elems.length; ++i) {{{Environment.NewLine}"
                + $"            elems[i].style.display = elems[i].getAttribute('boa-group') === this.value ? '' : 'none';{Environment.NewLine}"
                + $"        }}{Environment.NewLine}    }}{Environment.NewLine}</script>");
        }
    }

    private static TAttribute? GetDataModelPropertyAttribute<TAttribute>(ModelExpression modelExpression) where TAttribute : Attribute
    {
        if (modelExpression.Metadata is not DefaultModelMetadata metadata)
        {
            return null;
        }

        foreach (var attr in metadata.Attributes.Attributes)
        {
            if (attr is TAttribute attribute)
            {
                return attribute;
            }
        }

        return null;
    }

    private static void AppendDivOpenTag(string className, string? groupName, bool visible, TagHelperOutput output)
    {
        output.Content.AppendHtml("<div class=\"");
        output.Content.Append(className);
        if (groupName != null)
        {
            output.Content.AppendHtml("\" boa-group=\"");
            output.Content.Append(groupName);
        }
        if (!visible)
        {
            output.Content.AppendHtml("\" style=\"display: none");
        }
        output.Content.AppendHtml("\">");
    }

    private static void AppendLabelOpenTag(string fullName, string? className, TagHelperOutput output)
    {
        if (className != null)
        {
            output.Content.AppendHtml("<label class=\"");
            output.Content.Append(className);
            output.Content.AppendHtml("\" for=\"");
        }
        else
        {
            output.Content.AppendHtml("<label for=\"");
        }

        // toglie dal nome completo i caratteri non validi... nei tagHelper ufficiali il valore
        // viene cacheato in una classe internal, quindi purtroppo non posso utilizzare la cache
        output.Content.Append(TagBuilder.CreateSanitizedId(fullName, "_"));

        output.Content.AppendHtml("\">");
    }

    private static void AppendLabelText(ModelExpression model, TagHelperOutput output)
    {
        output.Content.Append(model.Metadata.DisplayName ?? model.Metadata.PropertyName);
    }

    private async Task AppendInputTagHelper(ModelExpression model, IList<TagHelperAttribute> attributes, TagHelperContext context, TagHelperOutput output)
    {
        TagHelper tagHelper = new InputTagHelper(_htmlGenerator)
        {
            For = model,
            ViewContext = ViewContext
        };
        output.Content.AppendHtml(await RenderTagHelper(tagHelper, "input", TagMode.SelfClosing, attributes, context.Items, null).ConfigureAwait(false));
    }

    private async Task AppendSelectTagHelper(ModelExpression model, IList<SelectListItem> items, IList<TagHelperAttribute> attributes, TagHelperContext context, TagHelperOutput output)
    {
        TagHelper tagHelper = new SelectTagHelper(_htmlGenerator)
        {
            For = model,
            Items = items,
            ViewContext = ViewContext
        };
        output.Content.AppendHtml(await RenderTagHelper(tagHelper, "select", TagMode.StartTagAndEndTag, attributes, context.Items, null).ConfigureAwait(false));
    }

    private async Task AppendValidationMessageTagHelper(ModelExpression model, TagHelperContext context, TagHelperOutput output)
    {
        TagHelper tagHelper = new ValidationMessageTagHelper(_htmlGenerator)
        {
            For = model,
            ViewContext = ViewContext
        };

        TagHelperAttribute[] attributes = new TagHelperAttribute[2] {
            new TagHelperAttribute("asp-validation-for", model),
            new TagHelperAttribute("class", "text-danger")
        };

        output.Content.AppendHtml(await RenderTagHelper(tagHelper, "span", TagMode.StartTagAndEndTag, attributes, context.Items, null).ConfigureAwait(false));
    }

    private async Task AppendCheckboxElement(ModelExpression model, string group, bool visible, TagHelperContext context, TagHelperOutput output)
    {
        if (Framework == UIFramework.Bootstrap4)
        {
            // aggiunge il div principale
            output.Content.AppendHtml(Environment.NewLine);
            AppendDivOpenTag("form-group", group, visible, output);

            // aggiunge il sotto-div
            output.Content.AppendHtml($"{Environment.NewLine}    <div class=\"checkbox\">");

            // aggiunge il label
            output.Content.AppendHtml($"{Environment.NewLine}        ");
            AppendLabelOpenTag(model.Name, null, output);

            // aggiunge l'input vero e proprio
            output.Content.AppendHtml($"{Environment.NewLine}            ");
            await AppendInputTagHelper(model, new TagHelperAttribute[1] {
                new TagHelperAttribute("asp-for", model)
            }, context, output).ConfigureAwait(false);

            // aggiunge il testo del label
            output.Content.AppendHtml($"{Environment.NewLine}            ");
            AppendLabelText(model, output);

            // chiude tutti i tag aperti
            output.Content.AppendHtml($"{Environment.NewLine}        </label>{Environment.NewLine}    </div>{Environment.NewLine}</div>");
            return;
        }

        // aggiunge il div principale
        output.Content.AppendHtml(Environment.NewLine);
        AppendDivOpenTag("checkbox mb-3", group, visible, output);

        // aggiunge il label
        output.Content.AppendHtml($"{Environment.NewLine}    ");
        AppendLabelOpenTag(model.Name, "form-label", output);

        // aggiunge il checkbox vero e proprio
        output.Content.AppendHtml($"{Environment.NewLine}        ");
        await AppendInputTagHelper(model, new TagHelperAttribute[2] {
            new TagHelperAttribute("class", "form-check-input"),
            new TagHelperAttribute("asp-for", model)
        }, context, output).ConfigureAwait(false);

        // aggiunge il testo del label
        output.Content.AppendHtml($"{Environment.NewLine}        ");
        AppendLabelText(model, output);

        // chiude il label e il div iniziale
        output.Content.AppendHtml($"{Environment.NewLine}    </label>{Environment.NewLine}</div>");
    }

    private async Task AppendInputElement(ModelExpression model, string group, bool visible, TagHelperContext context, TagHelperOutput output)
    {
        // crea la lista degli attributi da aggiungere all'input
        TagHelperAttributeList inputAttrs = new()
        {
            new TagHelperAttribute("asp-for", model),
            new TagHelperAttribute("class", "form-control")
        };

        // se tra le annotazioni del campo c'è un autocomplete lo aggiunge
        ResetPasswordAnnotationsAttribute? annotations = GetDataModelPropertyAttribute<ResetPasswordAnnotationsAttribute>(model);
        if (annotations != null && annotations.Autocomplete != null)
        {
            inputAttrs.Add(new TagHelperAttribute("autocomplete", annotations.Autocomplete));
        }

        // capire in che occasione devo aggiungere l'aria-requided
        //if (annotations != null && annotations.Autocomplete != null)
        //{
        //    // aggiungo la stringa true invece del boolean perché altrimenti verrebbe scritto con la T maiuscola
        //    inputAttrs.Add(new TagHelperAttribute("aria-required", "true"));
        //}

        if (Framework == UIFramework.Bootstrap4)
        {
            // aggiunge il div principale
            output.Content.AppendHtml(Environment.NewLine);
            AppendDivOpenTag("form-group", group, visible, output);

            // aggiunge il label
            output.Content.AppendHtml($"{Environment.NewLine}    ");
            AppendLabelOpenTag(model.Name, null, output);
            AppendLabelText(model, output);
            output.Content.AppendHtml($"</label>");

            // aggiunge l'input vero e proprio
            output.Content.AppendHtml($"{Environment.NewLine}    ");
            await AppendInputTagHelper(model, inputAttrs, context, output).ConfigureAwait(false);

            // aggiunge la riga dove scrivere eventuali errori
            output.Content.AppendHtml($"{Environment.NewLine}    ");
            await AppendValidationMessageTagHelper(model, context, output).ConfigureAwait(false);

            // chiude il label e il div iniziale
            output.Content.AppendHtml($"{Environment.NewLine}</div>");
            return;
        }

        // aggiunge il div principale
        output.Content.AppendHtml(Environment.NewLine);
        AppendDivOpenTag("form-floating mb-3", group, visible, output);

        // aggiunge l'input vero e proprio
        output.Content.AppendHtml($"{Environment.NewLine}    ");
        await AppendInputTagHelper(model, inputAttrs, context, output).ConfigureAwait(false);

        // aggiunge il label
        output.Content.AppendHtml($"{Environment.NewLine}    ");
        AppendLabelOpenTag(model.Name, "form-label", output);
        AppendLabelText(model, output);
        output.Content.AppendHtml($"</label>");

        // aggiunge la riga dove scrivere eventuali errori
        output.Content.AppendHtml($"{Environment.NewLine}    ");
        await AppendValidationMessageTagHelper(model, context, output).ConfigureAwait(false);

        // chiude il div iniziale
        output.Content.AppendHtml($"{Environment.NewLine}</div>");
    }

    private async Task AppendSelectElement(ModelExpression model, IList<SelectListItem> items, string? group, bool visible, TagHelperContext context, TagHelperOutput output)
    {
        // crea la lista degli attributi da aggiungere all'input
        TagHelperAttributeList selectAttrs = new() {
            new TagHelperAttribute("asp-for", model)
        };

        if (Framework == UIFramework.Bootstrap4)
        {
            // aggiunge il div principale
            output.Content.AppendHtml(Environment.NewLine);
            AppendDivOpenTag("form-group", group, visible, output);

            // aggiunge il label
            output.Content.AppendHtml($"{Environment.NewLine}    ");
            AppendLabelOpenTag(model.Name, null, output);
            AppendLabelText(model, output);
            output.Content.AppendHtml($"</label>");

            // aggiunge il select
            output.Content.AppendHtml($"{Environment.NewLine}    ");
            selectAttrs.Add(new TagHelperAttribute("class", "form-control"));
            await AppendSelectTagHelper(model, items, selectAttrs, context, output).ConfigureAwait(false);

            // chiude il div iniziale
            output.Content.AppendHtml($"{Environment.NewLine}</div>");
            return;
        }
        else
        {
            // aggiunge il div principale
            output.Content.AppendHtml(Environment.NewLine);
            AppendDivOpenTag("form-floating mb-3", group, visible, output);

            // aggiunge il select
            output.Content.AppendHtml($"{Environment.NewLine}    ");
            selectAttrs.Add(new TagHelperAttribute("class", "form-select"));
            await AppendSelectTagHelper(model, items, selectAttrs, context, output).ConfigureAwait(false);

            // aggiunge il label
            output.Content.AppendHtml($"{Environment.NewLine}    ");
            AppendLabelOpenTag(model.Name, "form-label", output);
            AppendLabelText(model, output);

            // chiude il label e il div iniziale
            output.Content.AppendHtml($"</label>{Environment.NewLine}</div>");
        }
    }

    private static async Task<string> RenderTagHelper(TagHelper tagHelper, string tagName, TagMode tagMode, IList<TagHelperAttribute> allAttributes, IDictionary<object, object> items, string? innerHtml)
    {
        // creo un nuovo contesto da usare per creare l'elemento d'uscita
        TagHelperContext context = new(new TagHelperAttributeList(allAttributes), items, "");

        // creo una lista di attributi da non copiare nell'elemento d'uscita
        List<string> toRemove = new();

        // scorro tutte le proprieta del tagHelper per vedere con che nome possono essere inserite nell'elemento d'ingresso
        foreach (var prop in tagHelper.GetType().GetProperties())
        {
            // se la proprietà non è scrivibile o ha l'attributo HtmlAttributeNotBoundAttribute non può essere passata in ingresso
            if (!prop.CanWrite || prop.GetCustomAttribute<HtmlAttributeNotBoundAttribute>() != null)
            {
                continue;
            }

            // verifico se la proprietà ha l'attributo HtmlAttributeNameAttribute
            var attrName = prop.GetCustomAttribute<HtmlAttributeNameAttribute>();

            // se ha l'attributo ed ha un nome specificato, uso quel nome, altrimenti uso il nome della proprietà
            string name = attrName != null && !string.IsNullOrEmpty(attrName.Name) ? attrName.Name : prop.Name;
            toRemove.Add(name.ToLowerInvariant());
        }

        // scorro tutte gli attributi passati in ingresso per vedere quali devo copiare in uscita
        var outAttrs = new TagHelperAttributeList();
        foreach (var attr in allAttributes)
        {
            bool toAdd = true;

            // cerco se l'attributo è tra quelli da non copiare in uscita
            string name = attr.Name.ToLowerInvariant();
            foreach (string s in toRemove)
            {
                if (name == s)
                {
                    toAdd = false;
                    break;
                }
            }

            // se l'elemento non è tra quelli da togliere, lo aggiungo alla lista d'uscita
            if (toAdd)
            {
                outAttrs.Add(attr);
            }
        }

        // creo la struttura dove aggiungere i dati dell'elemento d'uscita
        TagHelperOutput output = new(tagName, outAttrs, (useCachedResult, encoder) =>
        {
            var content = new DefaultTagHelperContent();
            if (!string.IsNullOrWhiteSpace(innerHtml))
            {
                content.AppendHtml(innerHtml);
            }
            return Task.FromResult<TagHelperContent>(content);
        })
        {
            TagMode = tagMode
        };

        // creo l'elemento e lo trasformo in stringa
        await tagHelper.ProcessAsync(context, output).ConfigureAwait(false);

        using StringWriter writer = new();
        output.WriteTo(writer, HtmlEncoder.Default);
        return writer.ToString();
    }
}
