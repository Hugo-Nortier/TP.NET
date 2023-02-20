using System.Globalization;
using System.IO;
using System.Reflection;
using System.Speech.Synthesis;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using Xceed.Wpf.Toolkit;

namespace WPF.Reader.Pages
{
    /// <summary>
    /// Interaction logic for ReadBook.xaml
    /// </summary>
    public partial class ReadBook : Page
    {
        private SpeechSynthesizer? synthsizer;

        public ReadBook()
        {
            InitializeComponent();
            InitializeEnabledState();
            this.synthsizer = null;
        }

        private void InitializeEnabledState()
        {
            this.btnsay.IsEnabled = true;
            this.btnpause.IsEnabled = false;
            this.btnstop.IsEnabled = true;
            this.btnplayselection.IsEnabled = false;
        }

        private void InitializeSynthsizer()
        {
            this.synthsizer = new SpeechSynthesizer();
            this.synthsizer.StateChanged += Synthsizer_StateChanged;
            this.synthsizer.SpeakCompleted += Synthsizer_SpeakCompleted;
            this.synthsizer.SpeakProgress += Synthsizer_SpeakProgress;
            this.synthsizer.SelectVoiceByHints(VoiceGender.Neutral, VoiceAge.NotSet, 0, CultureInfo.GetCultureInfo("fr-fr"));
        }

        private void Synthsizer_StateChanged(object? sender, StateChangedEventArgs e)
        {
            switch (e.State)
            {
                case SynthesizerState.Paused:
                    if (this.synthsizer != null)
                    {
                        this.synthsizer.Pause();
                    }
                    break;
            }
        }

        private void Synthsizer_SpeakCompleted(object? sender, SpeakCompletedEventArgs e)
        {
            if (this.synthsizer != null)
            {
                this.synthsizer.Dispose();
                this.synthsizer = null;
            }

            this.btnpause.Content = "⏸︎ Pause";
            this.btnsay.IsEnabled = true;
            this.Livre.IsInactiveSelectionHighlightEnabled = false;
            
            if (Livre.Selection.Text.Length > 0)
                this.btnplayselection.IsEnabled = true;
            else
                this.btnplayselection.IsEnabled = false;
        }

        private void Synthsizer_SpeakProgress(object? sender, SpeakProgressEventArgs e)
        {
            if (btnplayselection.IsEnabled == false)
            {
                ClearHighLight();
                this.Livre.Focus();
                TextRange textRange = new TextRange(Livre.Document.ContentStart, Livre.Document.ContentEnd);
                textRange.Select(textRange.Start.GetPositionAtOffset(e.CharacterPosition), textRange.Start.GetPositionAtOffset(e.CharacterPosition + e.Text.Length));
                textRange.ApplyPropertyValue(TextElement.BackgroundProperty, (SolidColorBrush)new BrushConverter().ConvertFrom("#99C9EF"));
            }
            else // si on lit du texte sélectionné... on aimerait traduire la ligne commentée qui fonctionne en TextBlock vers du RichTextBlock...
                 // mais comment faire? Je n'ai pas trouvé.
            {
                this.Livre.Focus();
                //this.Livre.Select(this.Livre.SelectionStart, this.Livre.SelectionLength);
            }
        }
        private void ClearHighLight()
        {
            TextRange textRange = new TextRange(Livre.Document.ContentStart, Livre.Document.ContentEnd);
            textRange.ApplyPropertyValue(TextElement.BackgroundProperty, null);
        }
        private void Btnsay_Click(object sender, RoutedEventArgs e)
        {
            if (synthsizer != null)
            {
                synthsizer.Dispose();
            }
            if (Livre.Text.ToString().Trim().Length == 0)
            {
                Xceed.Wpf.Toolkit.MessageBox.Show("Sélectionnez du texte");
                return;
            }
            InitializeSynthsizer();

            if (synthsizer != null)
            {
                string leLivre = new TextRange(Livre.Document.ContentStart, Livre.Document.ContentEnd).Text;

                synthsizer.SpeakAsync(leLivre);
            }

            this.btnpause.IsEnabled = true;
            this.btnplayselection.IsEnabled = false;
        }

        private void Btnpause_Click(object sender, RoutedEventArgs e)
        {
            if (btnpause.Content.ToString() == "⏸︎ Pause")
            {
                if (this.synthsizer != null)
                {
                    this.btnsay.IsEnabled = false;
                    this.synthsizer.Pause();
                    this.btnpause.Content = "▶ Resume";
                }
            }
            else if (btnpause.Content.ToString() == "▶ Resume")
            {
                if (this.synthsizer != null)
                {
                    this.btnsay.IsEnabled = true;
                    this.synthsizer.Resume();
                    this.btnpause.Content = "⏸︎ Pause";
                }
            }
        }

        private void Btnplayselection_Click(object sender, RoutedEventArgs e)
        {
            if (Livre.Selection.Text.Length > 0)
            {
                PlaySelectedText(Livre.Selection.Text.ToString());
            }
        }

        private void PlaySelectedText(string selectedTxt)
        {
            if (synthsizer != null)
            {
                synthsizer.Dispose();
                synthsizer = null;
            }
            InitializeSynthsizer();
            this.synthsizer.SpeakAsync(selectedTxt);
        }

        private void Btnstop_Click(object sender, RoutedEventArgs e)
        {
            ClearHighLight();
            this.btnpause.Content = "⏸︎ Pause";
            this.btnpause.IsEnabled = false;
            this.btnsay.IsEnabled = true;
            this.Livre.IsInactiveSelectionHighlightEnabled = false;
            this.btnplayselection.IsEnabled = false;
            if (this.synthsizer != null)
            {
                this.synthsizer.Dispose();
                this.synthsizer = null;
            }
        }

        private void LivrePreviewMouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (this.synthsizer == null)
            {
               if (Livre.Selection.Text.Length > 0)
                {
                    btnplayselection.IsEnabled = true;
                }
                else
                {
                    btnplayselection.IsEnabled = false;
                }
            }
            else
            {
                btnplayselection.IsEnabled = false;
            }
        }

        /*
         *  solution du forum https://www.c-sharpcorner.com/UploadFile/nipuntomar/text-to-speech-with-highlight-text-support-in-wpf/
         *  n'a pas de latence... mais si le mot est présent plusieurs fois: saute des lignes. . .
         *  peut-être à refacto? ou s'inspirer? pour les futurs MBDS?
        void Synthsizer_SpeakProgress(object sender, SpeakProgressEventArgs e)
        {
            SolidColorBrush highlightColor = (SolidColorBrush) new BrushConverter().ConvertFrom("#99C9EF");
            HighlightWordInRichTextBox(e.Text, highlightColor);
        }

        private void HighlightWordInRichTextBox(string word, SolidColorBrush color)
        {
            TextRange textRange = new TextRange(Livre.Document.ContentStart, Livre.Document.ContentEnd);
            textRange.ApplyPropertyValue(TextElement.BackgroundProperty, null);            
            TextRange tr = FindWordFromPosition(Livre.CaretPosition, word);
            if (!object.Equals(tr, null))
            {
                Livre.CaretPosition = tr.End;
                tr.ApplyPropertyValue(TextElement.BackgroundProperty, color);
            }
        }
        private TextRange FindWordFromPosition(TextPointer position, string word)
        {
            while (position != null)
            {
                if (position.GetPointerContext(LogicalDirection.Forward) == TextPointerContext.Text)
                {
                    string textRun = position.GetTextInRun(LogicalDirection.Forward);
                    int indexInRun = textRun.IndexOf(word);
                    if (indexInRun >= 0)
                    {
                        TextPointer start = position.GetPositionAtOffset(indexInRun);
                        TextPointer end = start.GetPositionAtOffset(word.Length);
                        return new TextRange(start, end);
                    }
                }
                position = position.GetNextContextPosition(LogicalDirection.Forward);
            }
            return null;
        }
        */
    }
}