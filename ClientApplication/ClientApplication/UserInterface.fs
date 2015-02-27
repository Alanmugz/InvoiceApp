
namespace InvoiceApp
 
open System
open System.Drawing
open System.Windows.Forms

module UserInterface = 

    let InvoiceCurrencies = [|"USD";"EUR";"CAD";"GBP";"NZD";"JPY"|]
    let Merchants = [|"ANZ";"KLM";"Cebu";"Jet";"Eithad"|]

    // Create the form and a labels, datetimepickers and button
    let frm = new Form(Text = "Invoicing Application!", Height = 218, Width = 260)
    let lblDateFrom = new Label()
    let dtpDateFrom = new System.Windows.Forms.DateTimePicker()
    let lblDateTo = new Label()
    let dtpDateTo = new System.Windows.Forms.DateTimePicker()
    let lblInvoiceCurrency = new Label()
    let lblMerchant = new Label()
    let lblProfitMargin = new Label()
    let lblPleaseWait = new Label()
    let txtProfitMargin = new TextBox()
    let btnGenerateInvoice = new Button()
    let cboInvoiceCurrency = new ComboBox(DataSource = InvoiceCurrencies)
    let cboMerchant = new ComboBox(DataSource = Merchants)
    // 
    // lblDateFrom
    // 
    lblDateFrom.AutoSize <- true
    lblDateFrom.Location <- new System.Drawing.Point(12, 15)
    lblDateFrom.Name <- "lblDateFrom"
    lblDateFrom.Size <- new System.Drawing.Size(62, 13)
    lblDateFrom.TabIndex <- 0
    lblDateFrom.Text <- "Date From: "
    // 
    // dtpDateFrom
    // 
    dtpDateFrom.Location <- new System.Drawing.Point(105, 12);
    dtpDateFrom.Name <- "dtpDateFrom";
    dtpDateFrom.Size <- new System.Drawing.Size(117, 20);
    dtpDateFrom.TabIndex <- 1;
    dtpDateFrom.Value <- new DateTime(2015,1,20);
    // 
    // lblDateTo
    // 
    lblDateTo.AutoSize <- true
    lblDateTo.Location <- new System.Drawing.Point(12, 41)
    lblDateTo.Name <- "lblDateTo"
    lblDateTo.Size <- new System.Drawing.Size(62, 13)
    lblDateTo.TabIndex <- 0
    lblDateTo.Text <- "Date To: "
    // 
    // dtpDateTo
    // 
    dtpDateTo.Location <- new System.Drawing.Point(105, 38);
    dtpDateTo.Name <- "dtpDateTo";
    dtpDateTo.Size <- new System.Drawing.Size(117, 20);
    dtpDateTo.TabIndex <- 2;
    dtpDateTo.Value <- new DateTime(2015,1,21);
    // 
    // lblInvoiceCurrency
    // 
    lblInvoiceCurrency.AutoSize <- true;
    lblInvoiceCurrency.Location <- new System.Drawing.Point(12, 69);
    lblInvoiceCurrency.Name <- "lblInvoiceCurrency";
    lblInvoiceCurrency.Size <- new System.Drawing.Size(87, 13);
    lblInvoiceCurrency.TabIndex <- 0;
    lblInvoiceCurrency.Text <- "Invoice Currency";
    // 
    // lblMerchant
    // 
    lblMerchant.AutoSize <- true;
    lblMerchant.Location <- new System.Drawing.Point(12, 95);
    lblMerchant.Name <- "lblMerchant";
    lblMerchant.Size <- new System.Drawing.Size(52, 13);
    lblMerchant.TabIndex <- 0;
    lblMerchant.Text <- "Merchant";
    // 
    // txtProfitMargin
    // 
    txtProfitMargin.Location <- new System.Drawing.Point(106, 120);
    txtProfitMargin.Name <- "txtProfitMargin";
    txtProfitMargin.Size <- new System.Drawing.Size(115, 20);
    txtProfitMargin.TabIndex <- 5;
    txtProfitMargin.Text <- "2.3";
    // 
    // lblProfitMargin
    // 
    lblProfitMargin.AutoSize <- true;
    lblProfitMargin.Location <- new System.Drawing.Point(12, 121);
    lblProfitMargin.Name <- "txtProfitMargin";
    lblProfitMargin.Size <- new System.Drawing.Size(66, 13);
    lblProfitMargin.TabIndex <- 0;
    lblProfitMargin.Text <- "Profit Margin";
    // 
    // btnGenerateInvoice
    // 
    btnGenerateInvoice.Location <- new System.Drawing.Point(123, 146);
    btnGenerateInvoice.Name <- "btnGenerateInvoice";
    btnGenerateInvoice.Size <- new System.Drawing.Size(99, 23);
    btnGenerateInvoice.TabIndex <- 6;
    btnGenerateInvoice.Text <- "Generate Invoice";
    btnGenerateInvoice.UseVisualStyleBackColor <- true;
    // 
    // cboInvoiceCurrency
    // 
    cboInvoiceCurrency.FormattingEnabled <- true;
    cboInvoiceCurrency.Location <- new System.Drawing.Point(106, 64);
    cboInvoiceCurrency.Name <- "cboInvoiceCurrency";
    cboInvoiceCurrency.Size <- new System.Drawing.Size(116, 21);
    cboInvoiceCurrency.TabIndex <- 3;
    // 
    // cboMerchant
    // 
    cboMerchant.FormattingEnabled <- true;
    cboMerchant.Location <- new System.Drawing.Point(106, 93);
    cboMerchant.Name <- "cboMerchant";
    cboMerchant.Size <- new System.Drawing.Size(116, 21);
    cboMerchant.TabIndex <- 4;
    //
    //lblPleaseWait
    //
    lblPleaseWait.AutoSize <- true;
    lblPleaseWait.Location <- new System.Drawing.Point(12, 150);
    lblPleaseWait.Name <- "label6";
    lblPleaseWait.Size <- new System.Drawing.Size(64, 13);
    lblPleaseWait.TabIndex <- 0;
    lblPleaseWait.Text <- "Please Wait";
    lblPleaseWait.Visible <- true;
    lblPleaseWait.ForeColor <- SystemColors.Control;
    // 
    // Form
    // 
    frm.Controls.Add(lblDateFrom)
    frm.Controls.Add(dtpDateFrom)
    frm.Controls.Add(lblDateTo)
    frm.Controls.Add(dtpDateTo)
    frm.Controls.Add(lblInvoiceCurrency)
    frm.Controls.Add(lblMerchant)
    frm.Controls.Add(txtProfitMargin)
    frm.Controls.Add(lblProfitMargin)
    frm.Controls.Add(lblPleaseWait)
    frm.Controls.Add(btnGenerateInvoice)
    frm.Controls.Add(cboInvoiceCurrency)
    frm.Controls.Add(cboMerchant)
    frm.Show()

    let getMerchant merchant = 
        match merchant with 
        | "ANZ" -> 1
        | "KLM" -> 2
        | "Cebu" -> 3
        | "Jet" -> 4
        | "Eithad" -> 5
        | _ -> -1

    let getCurrency currency = 
        match currency with 
        | "USD" -> 1
        | "EUR" -> 2
        | "CAD" -> 3
        | "GBP" -> 4
        | "NZD" -> 5
        | "JPY" -> 6
        | _ -> -1   
  
    let processFormData () = 
        ZeroMQClient.client <| (dtpDateFrom.Value, 
                                dtpDateTo.Value, 
                                getCurrency <| cboInvoiceCurrency.SelectedItem.ToString(), 
                                getMerchant <| cboMerchant.SelectedItem.ToString(), 
                                Convert.ToDecimal(txtProfitMargin.Text))

    let evtMessages = 
        btnGenerateInvoice.Click
            |> Event.map (fun _ -> processFormData ())
