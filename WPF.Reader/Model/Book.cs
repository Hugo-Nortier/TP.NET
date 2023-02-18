﻿using CommunityToolkit.Mvvm.DependencyInjection;
using System.Collections.Generic;
using System.Windows.Input;
using WPF.Reader.Service;
using WPF.Reader.Model;
using WPF.Reader.ViewModel;

namespace WPF.Reader.Model
{
    public class Book
    {
        public int Id { get; set; }
        public string Nom { get; set; }
        public string Auteur { get; set; }
        public string Contenu { get; set; }
        public float Prix { get; set; }
        public List<GenreDto> Genres { get; set; }
        public string GenresToString {  get; set; }
        public void GenresInString()
        {
            GenresToString = "";
            for (int i=0; i< Genres.Count; i++)
            {
                if (i != Genres.Count-1) GenresToString += Genres[i].Nom + " ; ";
                else GenresToString += Genres[i].Nom;
            }
        }
        public ICommand GoToReadBook { get; init; } = new RelayCommand(x => {
            var service = Ioc.Default.GetRequiredService<INavigationService>();
            if (service.Frame.CanGoBack)
            {
                service.Frame.RemoveBackEntry();
                var entry = service.Frame.RemoveBackEntry();
                while (entry != null)
                {
                    entry = service.Frame.RemoveBackEntry();
                }
            }
            service.Navigate<ReadBook>(x);
        });

        public ICommand DetailsBook { get; init; } = new RelayCommand(book =>
        {
            var service = Ioc.Default.GetRequiredService<INavigationService>();
            if (service.Frame.CanGoBack)
            {
                service.Frame.RemoveBackEntry();
                var entry = service.Frame.RemoveBackEntry();
                while (entry != null)
                {
                    entry = service.Frame.RemoveBackEntry();
                }
            }
            service.Navigate<DetailsBook>(book);
        });
    }
}
