#!/usr/bin/env bash
set -euo pipefail

# Simple issue creator using gh + jq
# Reads JSON array: [{ "title": "...", "body": "...", "labels": ["a","b"] }]
# Usage:
#   REPO_SLUG="owner/repo" ./tools/issues_create_simple.sh issues_plan.json
#   (If REPO_SLUG unset, auto-detects from current git repo)

need() { command -v "$1" >/dev/null 2>&1 || { echo "Missing: $1" >&2; exit 1; }; }
need gh; need jq
gh auth status >/dev/null

JSON_FILE="${1:-}"
[[ -f "$JSON_FILE" ]] || { echo "Usage: $0 <issues.json>"; exit 1; }

REPO_SLUG="${REPO_SLUG:-$(gh repo view --json nameWithOwner -q .nameWithOwner)}"
echo "Repo: $REPO_SLUG"

DRY_RUN="${DRY_RUN:-0}"  # set DRY_RUN=1 to preview

# Iterate issues safely (-c makes each item compact single-line JSON)
jq -c '.[]' "$JSON_FILE" | while IFS= read -r item; do
  title="$(jq -r '.title' <<<"$item")"
  body="$(jq -r '.body // empty' <<<"$item")"

  # Build label flags
  mapfile -t labels < <(jq -r '.labels // [] | .[]' <<<"$item")
  label_args=()
  for l in "${labels[@]:-}"; do
    # trim whitespace
    l="${l#"${l%%[![:space:]]*}"}"; l="${l%"${l##*[![:space:]]}"}"
    [[ -n "$l" ]] && label_args+=( --label "$l" )
  done

  echo "-> Creating: $title  [labels: ${labels[*]:-none}]"

  if [[ "$DRY_RUN" == "1" ]]; then
    echo "   DRY_RUN: gh issue create -R \"$REPO_SLUG\" --title \"$title\" --body-file <temp> ${label_args[*]}"
    continue
  fi

  # Use a temp file for body to preserve newlines and avoid shell quoting issues
  body_file=""
  if [[ -n "$body" ]]; then
    body_file="$(mktemp)"
    printf "%s" "$body" > "$body_file"
  fi

  if [[ -n "$body_file" ]]; then
    gh issue create -R "$REPO_SLUG" --title "$title" --body-file "$body_file" "${label_args[@]}"
    rm -f "$body_file"
  else
    gh issue create -R "$REPO_SLUG" --title "$title" "${label_args[@]}"
  fi

  # Small delay to be nice to the API
  sleep 1
done

echo "Done."
