#!/bin/bash

# Funkcja wyświetlająca pomoc
usage() {
    echo "Użycie: $0 [-v] [-fix] -len N [-case] [PLIK]"
    echo "  -v     (opcjonalnie) tryb verbose - wypisuje błędne linie"
    echo "  -fix   (opcjonalnie) usuń linie o nieprawidłowej długości i popraw wielkość liter"
    echo "  -len N  oczekiwana długość słów (liczba znaków)"
    echo "  -case  (opcjonalnie) sprawdź czy słowa są WIELKIMI literami"
    echo "  PLIK   (opcjonalnie) ścieżka do pliku; jeśli nie podano, czytane z stdin"
    exit 1
}

# Inicjalizacja zmiennych
verbose=0
fix_mode=0
check_case=0
expected_len=""
file=""

# Parsowanie argumentów
while [[ $# -gt 0 ]]; do
    case $1 in
        -v)
            verbose=1
            shift
            ;;
        -fix)
            fix_mode=1
            shift
            ;;
        -case)
            check_case=1
            shift
            ;;
        -len)
            expected_len="$2"
            shift 2
            ;;
        -*)
            echo "Nieznana opcja: $1"
            usage
            ;;
        *)
            if [[ -z "$file" ]]; then
                file="$1"
                shift
            else
                echo "Za dużo argumentów: $1"
                usage
            fi
            ;;
    esac
done

# Sprawdzenie, czy podano długość
if [[ -z "$expected_len" ]]; then
    echo "Musisz podać długość słów za pomocą -len"
    usage
fi

# Sprawdzenie, czy expected_len jest liczbą
if ! [[ "$expected_len" =~ ^[0-9]+$ ]]; then
    echo "Długość musi być liczbą całkowitą nieujemną: $expected_len"
    exit 1
fi

# Funkcja sprawdzająca, czy słowo jest w całości WIELKIMI literami
is_uppercase() {
    local word="$1"
    # Konwertuj na wielkie litery i porównaj z oryginałem
    local upper_word=$(echo "$word" | awk '{print toupper($0)}')
    if [[ "$word" == "$upper_word" ]]; then
        return 0  # true - słowo jest już WIELKIMI literami
    else
        return 1  # false - znaleziono małe litery
    fi
}

# Funkcja konwertująca słowo na WIELKIE litery (z obsługą polskich znaków)
to_uppercase() {
    local word="$1"
    echo "$word" | awk '{print toupper($0)}'
}

# Funkcja sprawdzająca plik
check_file() {
    local line_number=0
    local length_errors=0
    local case_errors=0
    local input="$1"
    
    # Jeśli tryb naprawy, przygotuj plik tymczasowy
    if [[ $fix_mode -eq 1 && -n "$file" ]]; then
        temp_file=$(mktemp)
    fi

    while IFS= read -r line; do
        line_number=$((line_number + 1))
        # Usunięcie ewentualnych białych znaków (w tym znaku nowej linii)
        word=$(echo "$line" | tr -d '\r\n' | sed 's/^[[:space:]]*//;s/[[:space:]]*$//')
        
        # Sprawdzenie długości
        len=${#word}
        length_ok=1
        case_ok=1
        
        if [[ $len -ne $expected_len ]]; then
            length_errors=$((length_errors + 1))
            length_ok=0
            if [[ $verbose -eq 1 ]]; then
                echo "Błąd długości w linii $line_number: \"$word\" (długość: $len, oczekiwana: $expected_len)" >&2
            fi
        fi
        
        # Sprawdzenie wielkości liter
        if [[ $check_case -eq 1 ]]; then
            if ! is_uppercase "$word"; then
                case_errors=$((case_errors + 1))
                case_ok=0
                if [[ $verbose -eq 1 ]]; then
                    echo "Błąd wielkości liter w linii $line_number: \"$word\" (oczekiwane WIELKIE litery)" >&2
                fi
            fi
        fi
        
        # Logika dla trybu naprawy
        if [[ $fix_mode -eq 1 ]]; then
            if [[ $length_ok -eq 1 ]]; then
                # Słowo ma dobrą długość
                local output_word="$word"
                
                # Jeśli mamy sprawdzać case i słowo ma zły case, popraw je
                if [[ $check_case -eq 1 && $case_ok -eq 0 ]]; then
                    output_word=$(to_uppercase "$word")
                    if [[ $verbose -eq 1 ]]; then
                        echo "Poprawiono wielkość liter w linii $line_number: \"$word\" -> \"$output_word\"" >&2
                    fi
                fi
                
                # Zapisz poprawne słowo
                if [[ -n "$file" ]]; then
                    echo "$output_word" >> "$temp_file"
                else
                    echo "$output_word"
                fi
            fi
        fi
    done < "$input"

    # Jeśli tryb naprawy i mamy plik, zastąp oryginalny plik
    if [[ $fix_mode -eq 1 && -n "$file" ]]; then
        local total_errors=$((length_errors + case_errors))
        if [[ $total_errors -gt 0 ]]; then
            mv "$temp_file" "$file"
            if [[ $verbose -eq 1 ]]; then
                echo "Naprawiono plik: usunięto $length_errors linii z nieprawidłową długością" >&2
                if [[ $check_case -eq 1 ]]; then
                    echo "Poprawiono $case_errors słów z nieprawidłową wielkością liter" >&2
                fi
            fi
        else
            rm "$temp_file"
            if [[ $verbose -eq 1 ]]; then
                echo "Wszystkie linie są poprawne. Plik nie wymagał zmian." >&2
            fi
        fi
    fi

    # Raport dla trybu walidacji (bez naprawy)
    if [[ $fix_mode -eq 0 ]]; then
        local total_errors=$((length_errors + case_errors))
        if [[ $total_errors -eq 0 ]]; then
            echo "Wszystkie słowa mają długość $expected_len znaków"
            if [[ $check_case -eq 1 ]]; then
                echo "Wszystkie słowa są zapisane WIELKIMI literami"
            fi
            exit 0
        else
            echo "Liczba błędów długości: $length_errors" >&2
            if [[ $check_case -eq 1 ]]; then
                echo "Liczba błędów wielkości liter: $case_errors" >&2
            fi
            exit 1
        fi
    else
        # W trybie naprawy zawsze kończymy z kodem 0 (operacja się udała)
        exit 0
    fi
}

# Sprawdzenie, czy plik istnieje (jeśli podano) lub czytanie z stdin
if [[ -n "$file" ]]; then
    if [[ ! -f "$file" ]]; then
        echo "Plik nie istnieje: $file" >&2
        exit 1
    fi
    check_file "$file"
else
    # Czytanie z stdin
    check_file "/dev/stdin"
fi
